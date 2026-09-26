using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeumLimit
{
    /// <summary>
    /// Limit B: how much faster the reference run's own route could be played by plain movement.
    ///
    /// Two sources, both on stretches of ordinary running and jumping - teleport rides, roar
    /// launches and anything else above walking speed are kept exactly as the reference played
    /// them, because they are not modelled yet:
    /// <list type="bullet">
    /// <item><b>Speed deficit.</b> An "ideal" speed is carried along the same path: never below the
    /// real one, gaining at most the character's acceleration per tick (60 m/s^2 on the ground,
    /// 30 in the air) up to the cap, and paying for turns the way the character controller does -
    /// the velocity vector itself can only change by acceleration * dt per tick, so a sharp turn
    /// at speed is slow for the ideal runner too.</item>
    /// <item><b>Shortcuts.</b> On the ground, a straight line to a later point of the path that the
    /// character's capsule can actually walk (checked against the live level by the caller).</item>
    /// </list>
    /// A sudden speed change no acceleration explains (a landing, a pad, a wall) starts a new
    /// stretch from the real speed and is not counted as loss: that part is conservative.
    /// </summary>
    internal static class PathLimit
    {
        internal struct Settings
        {
            internal float GroundCap;
            internal float AirCap;
            internal float GroundAcceleration;
            internal float AirAcceleration;
            internal float PoweredSpeed;
            internal int MaxShortcutTicks;

            internal static Settings Default
            {
                get
                {
                    return new Settings
                    {
                        GroundCap = 12.18f,          // measured plateau on flat ground
                        AirCap = 12f,                // movement.maxForwardSpeed
                        GroundAcceleration = 60f,    // movement.maxGroundAcceleration
                        AirAcceleration = 30f,       // movement.maxAirAcceleration
                        PoweredSpeed = 13f,          // above anything plain movement reaches
                        MaxShortcutTicks = 90,
                    };
                }
            }
        }

        internal sealed class Result
        {
            internal float ReferenceTime;
            internal float GroundLoss;
            internal float AirLoss;
            internal float ShortcutGain;
            internal float RoarGain;
            internal float TeleportGain;
            internal int TeleportsChecked;
            internal int TeleportsProbed;
            internal int RoarTicks;
            internal int LockedTicks;

            /// <summary>Ticks whose speed does not decide anything: see <see cref="NeutralTicks"/>.</summary>
            internal int NeutralTicks;
            internal int FreeTicks;
            internal int Shortcuts;

            /// <summary>Per tick: seconds the ideal runner saves on that tick (for drawing).</summary>
            internal float[] TickLoss;

            /// <summary>Air ticks' deficit, for information only: not part of the limit.</summary>
            internal float AirDeficit
            {
                get { return AirLoss; }
            }

            internal float Limit
            {
                get { return ReferenceTime - GroundLoss - ShortcutGain - RoarGain - TeleportGain; }
            }
        }

        /// <param name="isWalkable">From, to: whether the character can walk that straight line.
        /// Null outside the game, where there is no level to ask.</param>
        /// <param name="clearDistance">From, to: how far the character can move along that line
        /// before hitting something solid or deadly. Null outside the game.</param>
        internal static Result Compute(RunTrack run, Settings settings, Func<Vector3, Vector3, bool> isWalkable,
            Func<Vector3, Vector3, float> clearDistance = null)
        {
            int n = run.FrameCount;
            float dt = run.Dt;
            Result result = new Result();
            result.ReferenceTime = run.FinishTime;
            result.TickLoss = new float[n];

            bool[] grounded = GroundedTicks(run);
            bool[] locked = LockedTicks(run, settings);
            bool[] neutral = NeutralTicks(run);

            float ideal = -1f;              // ideal horizontal speed on the previous tick, -1 = no stretch
            Vector3 previousDirection = Vector3.zero;
            float previousActual = 0f;

            for (int i = 0; i + 1 < n; i++)
            {
                Vector3 step = run.Positions[i + 1] - run.Positions[i];
                Vector3 flat = new Vector3(step.x, 0f, step.z);
                float distance = flat.magnitude;
                float actual = distance / dt;

                if (locked[i] || neutral[i] || distance < 1e-4f)
                {
                    result.LockedTicks += locked[i] ? 1 : 0;
                    result.NeutralTicks += neutral[i] && !locked[i] ? 1 : 0;
                    ideal = -1f;
                    continue;
                }

                Vector3 direction = flat / distance;
                float acceleration = grounded[i] ? settings.GroundAcceleration : settings.AirAcceleration;
                float cap = grounded[i] ? settings.GroundCap : settings.AirCap;
                float change = acceleration * dt;

                bool external = ideal >= 0f && Mathf.Abs(actual - previousActual) > 1.2f * settings.GroundAcceleration * dt;
                if (ideal < 0f || external)
                {
                    ideal = actual;
                }
                else
                {
                    // The largest speed along the new direction reachable from the ideal velocity
                    // on the old one by a change of at most `change`: |m*u2 - v*u1| <= change.
                    float cos = Mathf.Clamp(Vector3.Dot(previousDirection, direction), -1f, 1f);
                    float sin2 = 1f - cos * cos;
                    float disc = change * change - ideal * ideal * sin2;
                    float reachable = disc >= 0f ? ideal * cos + Mathf.Sqrt(disc) : 0f;

                    // Above the cap the controller pulls speed back down at the same rate.
                    float target = ideal > cap ? Mathf.Max(cap, ideal - change) : Mathf.Min(cap, ideal + change);
                    ideal = Mathf.Max(actual, Mathf.Min(reachable, target));
                }

                // In the air the clock belongs to the vertical motion: a jump or a bounce lands
                // when gravity says so, and a faster horizontal speed only changes where. So air
                // ticks save nothing themselves - the speed the ideal runner builds up there is
                // carried onto the ground after the landing, where it does count.
                float saved = dt - distance / ideal;
                if (saved > 0f)
                {
                    if (grounded[i])
                    {
                        result.TickLoss[i] = saved;
                        result.GroundLoss += saved;
                    }
                    else
                    {
                        result.AirLoss += saved;
                    }
                }

                result.FreeTicks++;
                previousDirection = direction;
                previousActual = actual;
            }

            if (IsRoarRun(run))
            {
                Roar(run, locked, result);
            }

            Teleport(run, settings, grounded, result, clearDistance);

            if (isWalkable != null)
            {
                Shortcuts(run, settings, grounded, locked, result, isWalkable);
            }

            return result;
        }

        // Roar, from FPSInputController.handleHandInput and CharacterMotor.performFixedUpdate:
        // while a roar is active (at most 0.5 s per press, 3 presses until the ground is touched)
        // roarVector moves towards camera forward * 70 at 300 m/s^2; after it ends it drains at
        // 200 m/s^2. Above 30 m/s the roar alone moves the character, in a straight line along it.
        private const float RoarCap = 70f;
        private const float RoarAcceleration = 300f;
        private const float RoarDrain = 200f;

        /// <summary>A drop shorter than this, followed by growth, is a release and re-press.</summary>
        private const int RoarRepressTicks = 3;

        private static bool IsRoarRun(RunTrack run)
        {
            foreach (RunEvent e in run.Events)
            {
                if (e.Type == RunTrack.EventRoar || (e.Type == RunTrack.EventFire2 && e.Data < 0))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The roar stretches (the locked ticks of a roar run) played on the same line, with the
        /// roar held perfectly: growing at the full rate, turning no worse than the vector can,
        /// and re-pressed without a dip. What it does not invent is charges: where the real roar
        /// drains for longer than a re-press would take, the ideal one drains too.
        /// </summary>
        private static void Roar(RunTrack run, bool[] locked, Result result)
        {
            int n = run.FrameCount;
            float dt = run.Dt;
            float grow = RoarAcceleration * dt;
            float drain = RoarDrain * dt;

            float[] speed = new float[n];
            for (int i = 0; i + 1 < n; i++)
            {
                speed[i] = (run.Positions[i + 1] - run.Positions[i]).magnitude / dt;
            }

            // Long drains: the ideal runner drains through them as well.
            bool[] draining = new bool[n];
            for (int i = 1; i + 1 < n; )
            {
                if (speed[i] - speed[i - 1] >= -0.6f * drain)
                {
                    i++;
                    continue;
                }

                int j = i;
                while (j + 1 < n && speed[j] - speed[j - 1] < -0.6f * drain)
                {
                    j++;
                }

                bool repress = j - i <= RoarRepressTicks && j + 1 < n && speed[j] > speed[j - 1];
                for (int k = i; k < j; k++)
                {
                    draining[k] = !repress;
                }

                i = j;
            }

            float ideal = -1f;
            Vector3 previousDirection = Vector3.zero;
            for (int i = 0; i + 1 < n; i++)
            {
                if (!locked[i] || run.Teleports.Contains(i) || speed[i] < 1e-3f)
                {
                    ideal = -1f;
                    continue;
                }

                Vector3 step = run.Positions[i + 1] - run.Positions[i];
                Vector3 direction = step / step.magnitude;
                float actual = speed[i];

                if (ideal < 0f)
                {
                    ideal = actual;
                }
                else if (draining[i])
                {
                    ideal = Mathf.Max(actual, ideal - drain);
                }
                else
                {
                    float cos = Mathf.Clamp(Vector3.Dot(previousDirection, direction), -1f, 1f);
                    float disc = grow * grow - ideal * ideal * (1f - cos * cos);
                    float reachable = disc >= 0f ? ideal * cos + Mathf.Sqrt(disc) : 0f;
                    ideal = Mathf.Max(actual, Mathf.Min(reachable, RoarCap));
                }

                float saved = dt - step.magnitude / ideal;
                if (saved > 0f)
                {
                    result.RoarGain += saved;
                    result.TickLoss[i] += saved;
                }

                result.RoarTicks++;
                previousDirection = direction;
            }
        }

        /// <summary>
        /// Teleport, from FPSInputController.startFollowingPath / followPathUpdate: when the orb
        /// lands, the runner is put on the point of the orb's path nearest to them and replays the
        /// rest of it at k = max(3 * flight, 1.5) times the orb's pace. So every metre the runner
        /// covers along the orb's direction while it flies moves that point on, and shortens the
        /// ride by (metres / orb speed) / k. Measured on level 6: holding W through a 31-tick
        /// flight moved the snap point 2 orb ticks on and cut the ride from 21 to 18 ticks, as the
        /// formula says.
        ///
        /// For each teleport, the runner is given the best run-up along the orb's direction that
        /// acceleration allows from their real speed at the throw; the extra distance over what
        /// they really covered is the gain. The throw moment, the aim and the landing are the
        /// reference's own.
        /// </summary>
        private static void Teleport(RunTrack run, Settings settings, bool[] grounded, Result result,
            Func<Vector3, Vector3, float> clearDistance)
        {
            float dt = run.Dt;
            int n = run.FrameCount;

            foreach (int jump in run.Teleports)
            {
                // The throw that caused it: the last one before the jump.
                float thrown = -1f;
                foreach (float t in run.Throws)
                {
                    if (t < (jump + 1) * dt)
                    {
                        thrown = t;
                    }
                }

                if (thrown < 0f || jump + 3 >= n)
                {
                    continue;
                }

                int start = Mathf.Clamp(Mathf.RoundToInt(thrown / dt), 0, jump);
                int flight = jump + 1 - start;
                if (flight < 2)
                {
                    continue;
                }

                // The ride starts along the orb's path: its first ticks give the direction, and
                // the orb's pace there is the ride speed divided by k.
                Vector3 ride = run.Positions[jump + 2] - run.Positions[jump + 1];
                Vector3 along = new Vector3(ride.x, 0f, ride.z);
                if (along.sqrMagnitude < 1e-6f)
                {
                    continue;
                }

                along.Normalize();
                float d = flight * dt;
                float k = Mathf.Max(3f * d, 1.5f);
                float orbSpeed = ride.magnitude / dt / k;
                if (orbSpeed < 1f)
                {
                    continue;
                }

                float actual = Vector3.Dot(run.Positions[jump] - run.Positions[start], along);

                float v = Mathf.Max(0f, Vector3.Dot(run.Velocity(start), along));
                float ideal = 0f;
                for (int i = start; i < jump; i++)
                {
                    bool onGround = grounded[i];
                    float cap = onGround ? settings.GroundCap : settings.AirCap;
                    float acceleration = onGround ? settings.GroundAcceleration : settings.AirAcceleration;
                    v = v < cap ? Mathf.Min(cap, v + acceleration * dt) : Mathf.Max(cap, v - acceleration * dt);
                    ideal += v * dt;
                }

                result.TeleportsChecked++;
                float extra = ideal - actual;

                // The run-up has to fit: from where the runner was when the orb landed, the extra
                // metres go on along the orb's direction until a wall or a hazard stops them.
                if (extra > 0f && clearDistance != null)
                {
                    Vector3 at = run.Positions[jump];
                    extra = Mathf.Min(extra, clearDistance(at, at + along * extra));
                    result.TeleportsProbed++;
                }

                if (extra > 0f)
                {
                    result.TeleportGain += extra / orbSpeed / k;
                }
            }
        }

        /// <summary>
        /// String-pulls the grounded free stretches: from each point, the farthest later point
        /// (same stretch, walkable in a straight line) replaces the path in between. The gain is
        /// the length saved at the ideal speed over that piece.
        /// </summary>
        private static void Shortcuts(RunTrack run, Settings settings, bool[] grounded, bool[] locked,
            Result result, Func<Vector3, Vector3, bool> isWalkable)
        {
            int n = run.FrameCount;
            int i = 0;
            while (i + 1 < n)
            {
                if (locked[i] || !grounded[i])
                {
                    i++;
                    continue;
                }

                int end = i + 1;
                while (end + 1 < n && end - i < settings.MaxShortcutTicks && !locked[end] && grounded[end])
                {
                    end++;
                }

                int best = -1;
                for (int j = end; j > i + 1; j--)
                {
                    if (isWalkable(run.Positions[i], run.Positions[j]))
                    {
                        best = j;
                        break;
                    }
                }

                if (best < 0)
                {
                    i++;
                    continue;
                }

                float along = 0f;
                float time = 0f;
                for (int k = i; k < best; k++)
                {
                    Vector3 s = run.Positions[k + 1] - run.Positions[k];
                    along += new Vector2(s.x, s.z).magnitude;
                    time += run.Dt - result.TickLoss[k];
                }

                Vector3 d = run.Positions[best] - run.Positions[i];
                float straight = new Vector2(d.x, d.z).magnitude;
                if (along > 0f && straight < along - 0.01f)
                {
                    // Same average speed as the ideal runner had there, over the shorter line.
                    result.ShortcutGain += time * (1f - straight / along);
                    result.Shortcuts++;
                }

                i = best;
            }
        }

        /// <summary>
        /// Ticks where moving faster would not finish sooner, so a slow tick there is no loss:
        /// while a teleport orb is flying (the teleport happens when the orb lands, however fast
        /// the runner goes), and on the way to a throw or a trigger (what decides those is when
        /// the runner aims and shoots, which the movement model does not know). Movement counts
        /// when it is what takes the runner to the finish, a landing or a pickup.
        /// </summary>
        private static bool[] NeutralTicks(RunTrack run)
        {
            // Throws and triggers are "neutral" anchors, landings and pickups "movement" ones;
            // the finish is movement unless the run ends on a thrown orb.
            List<KeyValuePair<float, bool>> anchors = new List<KeyValuePair<float, bool>>();
            foreach (float thrown in run.Throws)
            {
                anchors.Add(new KeyValuePair<float, bool>(thrown, true));
            }

            foreach (RunEvent e in run.Events)
            {
                if (e.Type == RunTrack.EventTrigger)
                {
                    anchors.Add(new KeyValuePair<float, bool>(e.Time, true));
                }
                else if (e.Type == RunTrack.EventOnLand || e.Type == RunTrack.EventPickup
                    || e.Type == RunTrack.EventPickupProjectile)
                {
                    anchors.Add(new KeyValuePair<float, bool>(e.Time, false));
                }
            }

            anchors.Sort((a, b) => a.Key.CompareTo(b.Key));

            bool[] neutral = new bool[run.FrameCount];
            for (int i = 0; i < neutral.Length; i++)
            {
                float t = i * run.Dt;
                if (run.OrbInFlight(t))
                {
                    neutral[i] = true;
                    continue;
                }

                foreach (KeyValuePair<float, bool> anchor in anchors)
                {
                    if (anchor.Key > t)
                    {
                        neutral[i] = anchor.Value;
                        break;
                    }
                }
            }

            return neutral;
        }

        /// <summary>Ground state per tick from ON_LAND / UNGROUND; the spawn starts in the air.</summary>
        private static bool[] GroundedTicks(RunTrack run)
        {
            bool[] grounded = new bool[run.FrameCount];
            List<RunEvent> changes = new List<RunEvent>();
            foreach (RunEvent e in run.Events)
            {
                if (e.Type == RunTrack.EventOnLand || e.Type == RunTrack.EventUnground)
                {
                    changes.Add(e);
                }
            }

            bool state = false;
            int next = 0;
            for (int i = 0; i < grounded.Length; i++)
            {
                float t = (i + 0.5f) * run.Dt;
                while (next < changes.Count && changes[next].Time <= t)
                {
                    state = changes[next].Type == RunTrack.EventOnLand;
                    next++;
                }

                grounded[i] = state;
            }

            return grounded;
        }

        /// <summary>
        /// Ticks the model leaves alone: faster than plain movement goes (roar, teleport ride,
        /// launches), and the teleport jumps themselves.
        /// </summary>
        private static bool[] LockedTicks(RunTrack run, Settings settings)
        {
            bool[] locked = new bool[run.FrameCount];
            for (int i = 0; i + 1 < run.FrameCount; i++)
            {
                Vector3 s = run.Positions[i + 1] - run.Positions[i];
                if (new Vector2(s.x, s.z).magnitude / run.Dt > settings.PoweredSpeed)
                {
                    locked[i] = true;
                }
            }

            foreach (int tick in run.Teleports)
            {
                locked[tick] = true;
            }

            return locked;
        }
    }
}
