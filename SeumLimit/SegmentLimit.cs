using System.Collections.Generic;
using UnityEngine;

namespace SeumLimit
{
    /// <summary>
    /// Limit A: the best time that can be spliced together from pieces the top runs actually
    /// played.
    ///
    /// Checkpoints are laid along the reference run (the world record) every few ticks. Every run
    /// is then timed through each checkpoint by where it crosses the checkpoint's plane. The limit
    /// is a shortest path over (checkpoint, run): continuing with run b after reaching checkpoint k
    /// with run a is allowed only when the two were in the same state there - close together,
    /// moving alike, with the same triggers lit, the same pickups taken and the same teleport orb
    /// situation. That keeps the splice honest: every piece was played, and it was played from a
    /// state the previous piece really leaves you in.
    /// </summary>
    internal static class SegmentLimit
    {
        internal struct Settings
        {
            internal int CheckpointTicks;
            internal float PlaneRadius;
            internal float SwitchDistance;
            internal float SwitchSpeed;
            internal float SwitchSpeedGain;

            /// <summary>Extra tolerance per m/s of speed: 0.25 m/s decides a lot at 3 m/s after a
            /// bounce, and is below the noise of a teleport ride at 60.</summary>
            internal float SwitchSpeedRelative;

            internal static Settings Default
            {
                get
                {
                    return new Settings
                    {
                        CheckpointTicks = 10,
                        PlaneRadius = 3f,
                        SwitchDistance = 0.75f,
                        SwitchSpeed = 0.25f,
                        SwitchSpeedRelative = 0.05f,
                        SwitchSpeedGain = 0.25f,
                    };
                }
            }
        }

        internal sealed class Segment
        {
            internal float StartTime;   // on the spliced clock
            internal float EndTime;
            internal float ReferenceDuration;
            internal float ReferenceStart;  // on the reference's own clock, NaN if it missed
            internal float ReferenceEnd;
            internal float Duration;
            internal int Run;           // index into the runs passed in
        }

        internal sealed class Result
        {
            internal float Limit;
            internal float ReferenceTime;
            internal int ReferenceRun;
            internal int Checkpoints;
            internal int Switches;
            internal List<Segment> Segments = new List<Segment>();
        }

        private struct Crossing
        {
            internal bool Hit;
            internal float Time;
            internal Vector3 Position;
            internal Vector3 Velocity;
            internal int Triggers;
            internal int Pickups;
            internal int Pending;
            internal bool Orb;
        }

        /// <param name="runs">Runs of one leaderboard; runs[reference] defines the checkpoints.</param>
        internal static Result Compute(IList<RunTrack> runs, int reference, Settings settings)
        {
            RunTrack refRun = runs[reference];

            List<Vector3> points;
            List<Vector3> normals;
            Checkpoints(refRun, settings, out points, out normals);

            int n = points.Count;

            // crossings[r][k]: run r at checkpoint k. Index 0 is the start (spawn, t = 0) and
            // n + 1 the finish, both shared by every run by definition.
            Crossing[][] crossings = new Crossing[runs.Count][];
            for (int r = 0; r < runs.Count; r++)
            {
                RunTrack run = runs[r];
                Crossing[] row = new Crossing[n + 2];
                row[0] = State(run, 0f, run.Positions[0], run.Velocity(0));
                row[n + 1] = State(run, run.FinishTime, run.Positions[run.FrameCount - 1], run.Velocity(run.FrameCount - 2));

                // Walk forward only: a run has to pass the checkpoints in order.
                int from = 0;
                for (int k = 0; k < n; k++)
                {
                    int tick;
                    float fraction;
                    if (FindCrossing(run, from, points[k], normals[k], settings.PlaneRadius, out tick, out fraction))
                    {
                        Vector3 p = Vector3.Lerp(run.Positions[tick], run.Positions[tick + 1], fraction);
                        row[k + 1] = State(run, (tick + fraction) * run.Dt, p, run.Velocity(tick));
                        from = tick + 1;
                    }
                }

                crossings[r] = row;
            }

            // best[k, r]: fastest spliced time to checkpoint k arriving as run r.
            int total = n + 2;
            float[,] best = new float[total, runs.Count];
            int[,] cameFrom = new int[total, runs.Count];
            int[,] prevPoint = new int[total, runs.Count];
            for (int k = 0; k < total; k++)
            {
                for (int r = 0; r < runs.Count; r++)
                {
                    best[k, r] = float.PositiveInfinity;
                    cameFrom[k, r] = -1;
                    prevPoint[k, r] = -1;
                }
            }

            for (int r = 0; r < runs.Count; r++)
            {
                best[0, r] = 0f;
            }

            for (int k = 0; k < total - 1; k++)
            {
                for (int a = 0; a < runs.Count; a++)
                {
                    if (float.IsInfinity(best[k, a]))
                    {
                        continue;
                    }

                    for (int b = 0; b < runs.Count; b++)
                    {
                        // Everyone starts at the spawn, but not necessarily in the same state
                        // (a first throw already in the air), so the start is no exception.
                        if (a != b && !Compatible(crossings[a][k], crossings[b][k], settings))
                        {
                            continue;
                        }

                        // Continue run b to its next checkpoint - the next one it actually hit.
                        for (int next = k + 1; next < total; next++)
                        {
                            if (!crossings[b][next].Hit)
                            {
                                continue;
                            }

                            float t = best[k, a] + SwitchPenalty(crossings[a][k], crossings[b][k], a == b)
                                + crossings[b][next].Time - crossings[b][k].Time;
                            if (t < best[next, b])
                            {
                                best[next, b] = t;
                                cameFrom[next, b] = a;
                                prevPoint[next, b] = k;
                            }

                            break;
                        }
                    }
                }
            }

            Result result = new Result();
            result.ReferenceTime = refRun.FinishTime;
            result.ReferenceRun = reference;
            result.Checkpoints = n;
            result.Limit = float.PositiveInfinity;
            int end = -1;
            for (int r = 0; r < runs.Count; r++)
            {
                if (best[total - 1, r] < result.Limit)
                {
                    result.Limit = best[total - 1, r];
                    end = r;
                }
            }

            // Walk the path back into segments.
            int point = total - 1;
            int runIndex = end;
            while (runIndex >= 0 && point > 0)
            {
                int k = prevPoint[point, runIndex];
                int a = cameFrom[point, runIndex];
                Segment s = new Segment();
                s.Run = runIndex;
                s.StartTime = best[k, a];
                s.EndTime = best[point, runIndex];
                s.Duration = s.EndTime - s.StartTime;
                s.ReferenceDuration = crossings[reference][point].Hit && crossings[reference][k].Hit
                    ? crossings[reference][point].Time - crossings[reference][k].Time
                    : float.NaN;
                s.ReferenceStart = crossings[reference][k].Hit ? crossings[reference][k].Time : float.NaN;
                s.ReferenceEnd = crossings[reference][point].Hit ? crossings[reference][point].Time : float.NaN;
                result.Segments.Insert(0, s);
                if (a != runIndex)
                {
                    result.Switches++;
                }

                point = k;
                runIndex = a;
            }

            return result;
        }

        /// <summary>
        /// Horizontal and vertical velocity are compared separately: right after a bounce the
        /// runner rises at 20 m/s but moves sideways at 2-3, and it is the sideways part that
        /// decides the next second - a tolerance scaled by the whole speed would wave it through.
        /// </summary>
        private static bool Close(Vector3 a, Vector3 b, Settings settings)
        {
            return (a - b).magnitude <= settings.SwitchSpeed + settings.SwitchSpeedRelative * a.magnitude;
        }

        private static Vector3 Flat(Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }

        /// <summary>
        /// What a switch really costs. Two crossings of the same plane are never at exactly the
        /// same spot: if b's is further along a's direction of travel, splicing would hand out
        /// that distance for free, so it is charged at a's speed. Being behind earns nothing back.
        /// Without this, a dozen switches each "gaining" part of the tolerance add up to a limit
        /// nobody can play.
        /// </summary>
        private static float SwitchPenalty(Crossing a, Crossing b, bool same)
        {
            if (same)
            {
                return 0f;
            }

            float speed = a.Velocity.magnitude;
            if (speed < 1f)
            {
                return 0f;
            }

            float ahead = Vector3.Dot(b.Position - a.Position, a.Velocity / speed);
            return ahead > 0f ? ahead / speed : 0f;
        }

        /// <summary>
        /// Checkpoint k sits at the reference position after k*step ticks, facing the way the
        /// reference was moving. Ticks inside a teleport jump are skipped: the "direction" there
        /// is the jump itself, and nothing else can be crossing that plane.
        /// </summary>
        private static void Checkpoints(RunTrack refRun, Settings settings, out List<Vector3> points, out List<Vector3> normals)
        {
            points = new List<Vector3>();
            normals = new List<Vector3>();
            for (int tick = settings.CheckpointTicks; tick < refRun.FrameCount - 1; tick += settings.CheckpointTicks)
            {
                if (refRun.Teleports.Contains(tick) || refRun.Teleports.Contains(tick - 1))
                {
                    continue;
                }

                Vector3 v = refRun.Velocity(tick);
                if (v.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                points.Add(refRun.Positions[tick]);
                normals.Add(v.normalized);
            }
        }

        /// <summary>A place on the reference's route and when each of the two runs got there.</summary>
        internal struct Split
        {
            internal float ReferenceTime;
            internal float OtherTime;
        }

        /// <summary>
        /// Times of <paramref name="other"/> at the reference's checkpoints (the ones it passed,
        /// in order), plus the finish. The difference other - reference, followed along the run,
        /// shows where one gained on the other.
        /// </summary>
        internal static List<Split> Splits(RunTrack reference, RunTrack other, Settings settings)
        {
            List<Vector3> points;
            List<Vector3> normals;
            Checkpoints(reference, settings, out points, out normals);

            List<Split> splits = new List<Split>();
            int fromRef = 0, fromOther = 0;
            for (int k = 0; k < points.Count; k++)
            {
                int tr, to;
                float fr, fo;
                if (!FindCrossing(reference, fromRef, points[k], normals[k], settings.PlaneRadius, out tr, out fr)
                    || !FindCrossing(other, fromOther, points[k], normals[k], settings.PlaneRadius, out to, out fo))
                {
                    continue;
                }

                fromRef = tr + 1;
                fromOther = to + 1;
                splits.Add(new Split { ReferenceTime = (tr + fr) * reference.Dt, OtherTime = (to + fo) * other.Dt });
            }

            splits.Add(new Split { ReferenceTime = reference.FinishTime, OtherTime = other.FinishTime });
            return splits;
        }

        private static Crossing State(RunTrack run, float time, Vector3 position, Vector3 velocity)
        {
            return new Crossing
            {
                Hit = true,
                Time = time,
                Position = position,
                Velocity = velocity,
                Triggers = run.TriggersBy(time),
                Pickups = run.PickupsBy(time),
                Pending = run.PendingTriggers(time),
                Orb = run.OrbInFlight(time),
            };
        }

        /// <summary>
        /// Whether the rest of run b can follow run a from here. Nothing may be in the air:
        /// where a teleport orb lands and what a fireball lights were decided when they were
        /// thrown, so switching runs while one is flying would borrow the other run's throw.
        /// </summary>
        private static bool Compatible(Crossing a, Crossing b, Settings settings)
        {
            return a.Hit && b.Hit
                && a.Triggers == b.Triggers
                && a.Pickups == b.Pickups
                && !a.Orb && !b.Orb
                && a.Pending == 0 && b.Pending == 0
                && (a.Position - b.Position).magnitude <= settings.SwitchDistance
                && Close(Flat(a.Velocity), Flat(b.Velocity), settings)
                && Close(new Vector3(0f, a.Velocity.y, 0f), new Vector3(0f, b.Velocity.y, 0f), settings)
                // Taking over a faster run would be free speed; slower is allowed (it only loses).
                && b.Velocity.magnitude <= a.Velocity.magnitude + settings.SwitchSpeedGain;
        }

        /// <summary>
        /// First tick at or after <paramref name="from"/> where the run goes from behind the
        /// checkpoint plane to in front of it, close enough to the checkpoint to count.
        /// </summary>
        private static bool FindCrossing(RunTrack run, int from, Vector3 point, Vector3 normal,
            float radius, out int tick, out float fraction)
        {
            for (int i = from; i + 1 < run.FrameCount; i++)
            {
                float d0 = Vector3.Dot(run.Positions[i] - point, normal);
                float d1 = Vector3.Dot(run.Positions[i + 1] - point, normal);
                if (d0 < 0f && d1 >= 0f)
                {
                    float f = d0 / (d0 - d1);
                    Vector3 p = Vector3.Lerp(run.Positions[i], run.Positions[i + 1], f);
                    if ((p - point).magnitude <= radius)
                    {
                        tick = i;
                        fraction = f;
                        return true;
                    }
                }
            }

            tick = -1;
            fraction = 0f;
            return false;
        }
    }
}
