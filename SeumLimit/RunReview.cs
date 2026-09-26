using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using UnityEngine;
using IOPath = System.IO.Path;

namespace SeumLimit
{
    /// <summary>
    /// After a finish: the run just played, against the top runs of the same board.
    ///
    /// Three views, all on the player's own clock and route:
    /// <list type="bullet">
    /// <item>Where it fell behind the world record - the time difference followed along the
    /// run, and the stretches where it grew the most.</item>
    /// <item>Limit A with this run as the base: the pieces of the top runs that are faster than
    /// the player's own from the same state.</item>
    /// <item>Limit B on this run: what running, roar and teleport would still give on the
    /// player's own line.</item>
    /// </list>
    /// </summary>
    internal static class RunReview
    {
        private const int WorstStretches = 3;
        private const float StepLoss = 0.002f;
        private static readonly string NL = "\n";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static int reviewedFinish;

        internal static string Status;
        internal static int SeenFrame = -1;

        internal static void Tick()
        {
            GameManager manager = TopRuns.CurrentManager();
            if (manager == null || manager.gameplayState != GameManager.GameplayState.FINISH_LEVEL)
            {
                return;
            }

            SeenFrame = Time.frameCount;

            // One review per finish: the game may hand out the same session object again, so the
            // finish counter, not the object, says whether this one is new.
            Replay.ReplaySession session = LiveRecorder.LastFinishSession;
            if (session == null || LiveRecorder.FinishCount == reviewedFinish || session.frameCount < 2)
            {
                return;
            }

            reviewedFinish = LiveRecorder.FinishCount;
            float finish = LiveRecorder.LastFinishMs > 0 ? LiveRecorder.LastFinishMs / 1000f : session.frameCount * Time.fixedDeltaTime;
            RunTrack own = TopRuns.ToTrack(session, "вы", 0, finish);

            bool haveTop = TopRuns.Loaded != null && TopRuns.Loaded.Count > 0
                && TopRuns.LoadedLevel == Game.currentLevel && TopRuns.LoadedMutator == LevelSelector.currentLevelMutator;

            LevelProbe probe = LevelProbe.Create();
            Status = Describe(own, haveTop ? TopRuns.Loaded : null, TopRuns.LoadedReference, probe);

            if (LimitConfig.KeepReviews.Value <= 0)
            {
                return;
            }

            try
            {
                string dir = IOPath.Combine(Paths.CachePath, IOPath.Combine("SeumLimit", "reviews"));
                Directory.CreateDirectory(dir);
                string name = DateTime.Now.ToString("yyyyMMdd_HHmmss", Inv) + "_L" + Game.currentLevel.ToString(Inv)
                    + "_m" + LevelSelector.currentLevelMutator.ToString(Inv) + ".txt";
                File.WriteAllText(IOPath.Combine(dir, name), Status, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not save the run review: " + e.Message);
            }
        }

        internal static string Describe(RunTrack own, List<RunTrack> top, int wrIndex, LevelProbe probe)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Inv, "Ваш забег {0:0.000}", own.FinishTime);

            if (top == null)
            {
                sb.Append("   (топ этой таблицы не загружен: откройте экран прицеливания и дождитесь лимита)\n");
            }
            else
            {
                RunTrack wr = top[wrIndex];
                sb.AppendFormat(Inv, "   WR {0:0.000} ({1}), вы {2:+0;-0;0} мс\n",
                    wr.FinishTime, wr.Name, (own.FinishTime - wr.FinishTime) * 1000f);
                DescribeAgainstLimit(sb, own, top);
            }

            PathLimit.Result path = PathLimit.Compute(own, PathLimit.Settings.Default,
                probe == null ? null : new Func<Vector3, Vector3, bool>(probe.IsWalkable),
                probe == null ? null : new Func<Vector3, Vector3, float>(probe.ClearDistance));
            sb.AppendFormat(Inv, "На вашей линии (B): {0:0.000} ({1:+0;-0;0} мс): разгон на земле −{2:0}, рор −{3:0}, телепорт −{4:0}, срезы −{5:0} мс",
                path.Limit, (path.Limit - path.ReferenceTime) * 1000f, path.GroundLoss * 1000f,
                path.RoarGain * 1000f, path.TeleportGain * 1000f, path.ShortcutGain * 1000f);

            return sb.ToString();
        }

        /// <summary>
        /// Limit A with the player's run as the base, and the player's run held against it piece
        /// by piece: on every stretch between checkpoints, the player's own time next to the best
        /// piece any top run played from the same state. The stretches that lose the most are the
        /// ones to work on - for a top-1 player this is the only comparison that means anything,
        /// the record being their own.
        /// </summary>
        private static void DescribeAgainstLimit(StringBuilder sb, RunTrack own, List<RunTrack> top)
        {
            List<RunTrack> runs = new List<RunTrack>(top);
            runs.Add(own);
            int ownIndex = runs.Count - 1;
            SegmentLimit.Result result = SegmentLimit.Compute(runs, ownIndex, SegmentLimit.Settings.Default);

            sb.AppendFormat(Inv, "Лимит A для вас: {0:0.000} ({1:+0;-0;0} мс)" + NL,
                result.Limit, (result.Limit - own.FinishTime) * 1000f);

            // Consecutive segments taken from the same run make one stretch; only stretches where
            // another run is faster by a noticeable margin are listed.
            List<float> from = new List<float>();
            List<float> to = new List<float>();
            List<float> lost = new List<float>();
            List<int> source = new List<int>();

            int i = 0;
            while (i < result.Segments.Count)
            {
                int run = result.Segments[i].Run;
                int j = i;
                float gain = 0f;
                while (j < result.Segments.Count && result.Segments[j].Run == run)
                {
                    gain += result.Segments[j].ReferenceDuration - result.Segments[j].Duration;
                    j++;
                }

                float start = result.Segments[i].ReferenceStart;
                float end = result.Segments[j - 1].ReferenceEnd;
                if (run != ownIndex && gain >= StepLoss && !float.IsNaN(start) && !float.IsNaN(end))
                {
                    from.Add(start);
                    to.Add(end);
                    lost.Add(gain);
                    source.Add(run);
                }

                i = j;
            }

            if (lost.Count == 0)
            {
                sb.Append("  ни на одном отрезке топ-" + top.Count + " не быстрее вас из того же состояния" + NL);
                return;
            }

            List<int> order = new List<int>();
            for (int k = 0; k < lost.Count; k++)
            {
                order.Add(k);
            }

            order.Sort((a, b) => lost[b].CompareTo(lost[a]));
            sb.Append("  где теряете к лимиту (время по вашему забегу):" + NL);
            float shown = 0f;
            for (int k = 0; k < order.Count && k < WorstStretches; k++)
            {
                int n = order[k];
                shown += lost[n];
                sb.AppendFormat(Inv, "    {0:0.000}–{1:0.000}: −{2:0} мс, лучше у #{3} {4}" + NL,
                    from[n], to[n], lost[n] * 1000f, runs[source[n]].Place, runs[source[n]].Name);
            }

            float rest = own.FinishTime - result.Limit - shown;
            if (rest >= StepLoss)
            {
                sb.AppendFormat(Inv, "    ещё {0:0} мс мелкими кусками по забегу" + NL, rest * 1000f);
            }
        }
    }
}
