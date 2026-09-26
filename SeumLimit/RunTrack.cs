using System.Collections.Generic;
using UnityEngine;

namespace SeumLimit
{
    /// <summary>A replay event moved onto the run's own clock.</summary>
    internal struct RunEvent
    {
        internal int Type;
        internal int Data;

        /// <summary>Seconds since the first input - the same clock as the frames.</summary>
        internal float Time;
    }

    /// <summary>
    /// One leaderboard run as the analysis sees it: a position per physics tick, the events on the
    /// same clock, and the time the leaderboard shows. Knows nothing about the game's own types,
    /// so the analysis can be run outside the game on dumped CSVs.
    ///
    /// Two quirks of the recording are handled here, once:
    /// <list type="bullet">
    /// <item>Event timestamps count from the level restart, frames from the first input; the gap
    /// is the session's waitTime.</item>
    /// <item>The input that starts the run is processed before recording is switched on, so if it
    /// was a throw or a roar it is missing from the events. <see cref="Teleports"/> recovers
    /// teleports from the trajectory instead, which also catches the missing first throw.</item>
    /// </list>
    /// </summary>
    internal sealed class RunTrack
    {
        internal const int EventFire2 = 1;
        internal const int EventTrigger = 5;
        internal const int EventPickup = 3;
        internal const int EventPickupProjectile = 4;
        internal const int EventOnLand = 10;
        internal const int EventUnground = 11;
        internal const int EventRoar = 13;

        /// <summary>Apparent speed of the jump tick: teleports measured 120-400 m/s.</summary>
        private const float TeleportSpeed = 110f;
        private const float TeleportContrast = 1.8f;

        internal readonly string Name;
        internal readonly int Place;
        internal readonly float Dt;
        internal readonly Vector3[] Positions;
        internal readonly RunEvent[] Events;

        /// <summary>The leaderboard time in seconds. Sub-tick: the game adds the fraction of the
        /// last tick it would take to reach the exit.</summary>
        internal readonly float FinishTime;

        /// <summary>Ticks on which the runner was teleported (position jump between i and i+1).</summary>
        internal readonly List<int> Teleports = new List<int>();

        /// <summary>Times of teleport throws: FIRE2 events with a projectile id, plus 0 when a
        /// teleport happens with no throw recorded before it - the lost first input.</summary>
        internal readonly List<float> Throws = new List<float>();

        internal RunTrack(string name, int place, float dt, float waitTime, Vector3[] positions,
            IList<RunEvent> rawEvents, float finishTime)
        {
            Name = name;
            Place = place;
            Dt = dt;
            Positions = positions;
            FinishTime = finishTime;

            Events = new RunEvent[rawEvents.Count];
            for (int i = 0; i < rawEvents.Count; i++)
            {
                RunEvent e = rawEvents[i];
                e.Time -= waitTime;
                Events[i] = e;
            }

            FindTeleports();
        }

        internal int FrameCount
        {
            get { return Positions.Length; }
        }

        internal Vector3 Velocity(int frame)
        {
            if (frame < 0)
            {
                frame = 0;
            }

            if (frame >= Positions.Length - 1)
            {
                frame = Positions.Length - 2;
            }

            return frame < 0 ? Vector3.zero : (Positions[frame + 1] - Positions[frame]) / Dt;
        }

        /// <summary>How many level triggers (candles, buttons) had fired by this time.</summary>
        internal int TriggersBy(float time)
        {
            int n = 0;
            foreach (RunEvent e in Events)
            {
                if (e.Type == EventTrigger && e.Time <= time)
                {
                    n++;
                }
            }

            return n;
        }

        /// <summary>
        /// A fireball is lit before it reaches the candle: the top runs show 0.5-0.65 s between
        /// the shot and the trigger. Within this window of a trigger the run is already committed
        /// to it, by a projectile the replay does not tie to the trigger.
        /// </summary>
        internal const float TriggerLead = 1f;

        /// <summary>Triggers that fire within <see cref="TriggerLead"/> after this time - most
        /// likely already on their way.</summary>
        internal int PendingTriggers(float time)
        {
            int n = 0;
            foreach (RunEvent e in Events)
            {
                if (e.Type == EventTrigger && e.Time > time && e.Time <= time + TriggerLead)
                {
                    n++;
                }
            }

            return n;
        }

        /// <summary>How many pickups (powerups, projectile pickups) had been collected by this time.</summary>
        internal int PickupsBy(float time)
        {
            int n = 0;
            foreach (RunEvent e in Events)
            {
                if ((e.Type == EventPickup || e.Type == EventPickupProjectile) && e.Time <= time)
                {
                    n++;
                }
            }

            return n;
        }

        /// <summary>
        /// True while a teleport orb is flying: thrown, and the teleport it causes is still ahead.
        /// Two runs in that state and in a different one cannot be spliced - one of them is about
        /// to be moved somewhere else.
        /// </summary>
        internal bool OrbInFlight(float time)
        {
            int throwIndex = 0;
            foreach (int tick in Teleports)
            {
                float teleportTime = (tick + 1) * Dt;

                // The throw that caused this teleport is the last one before it.
                float thrown = -1f;
                while (throwIndex < Throws.Count && Throws[throwIndex] < teleportTime)
                {
                    thrown = Throws[throwIndex];
                    throwIndex++;
                }

                if (thrown >= 0f && thrown <= time && time < teleportTime)
                {
                    return true;
                }
            }

            // A throw after the last teleport is still flying when the run ends: the finish
            // itself is that orb reaching the exit (the game times it by the orb's impact).
            float lastTeleport = Teleports.Count == 0 ? -1f : (Teleports[Teleports.Count - 1] + 1) * Dt;
            return Throws.Count > 0 && Throws[Throws.Count - 1] > lastTeleport
                && time >= Throws[Throws.Count - 1];
        }

        private void FindTeleports()
        {
            // A teleport is one tick whose apparent speed is far above anything movement reaches
            // (a roar launch tops out around 85 m/s, a teleport ride around 60) and far above the
            // ticks on either side of it. Checking both neighbours keeps a roar launch - fast, but
            // fast for several ticks - from reading as one, and flags each jump exactly once.
            for (int i = 0; i + 1 < Positions.Length; i++)
            {
                float speed = (Positions[i + 1] - Positions[i]).magnitude / Dt;
                float before = i > 0 ? (Positions[i] - Positions[i - 1]).magnitude / Dt : 0f;
                float after = i + 2 < Positions.Length ? (Positions[i + 2] - Positions[i + 1]).magnitude / Dt : 0f;
                if (speed > TeleportSpeed && speed > TeleportContrast * Mathf.Max(before, after))
                {
                    Teleports.Add(i);
                }
            }

            foreach (RunEvent e in Events)
            {
                if (e.Type == EventFire2 && e.Data >= 0)
                {
                    Throws.Add(e.Time);
                }
            }

            if (Teleports.Count > 0 && (Throws.Count == 0 || Throws[0] > Teleports[0] * Dt))
            {
                Throws.Insert(0, 0f);
            }
        }
    }
}
