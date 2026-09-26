using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using IOPath = System.IO.Path;

namespace SeumLimit
{
    /// <summary>
    /// Collects the top runs of the leaderboard the player is looking at and runs the analysis on
    /// them once they are all downloaded.
    ///
    /// Works on the aim screen, like SeumReplay's prefetch: the leaderboard response is there, and
    /// asking for the replays through <c>LeaderboardsBackend.downloadReplay</c> is exactly what
    /// opening them would do - with SeumReplay installed they come from its disk cache.
    ///
    /// Every collected run is also written to BepInEx/cache/SeumLimit/runs as CSV, so the analysis
    /// can be iterated on outside the game.
    /// </summary>
    internal static class TopRuns
    {
        private const float DownloadTimeoutSeconds = 45f;

        /// <summary>Bumped whenever the analysis changes, so cached results from an older
        /// version are not shown as if they were current.</summary>
        internal const int AnalysisVersion = 10;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static FieldInfo gameManagerField;

        private static string boardKey;
        private static Score[] pending;
        private static float requestedAt;

        /// <summary>What the overlay shows; null while there is nothing to show.</summary>
        internal static string Status;

        /// <summary>Board key the current <see cref="Status"/> belongs to.</summary>
        internal static string StatusBoard;

        /// <summary>Last frame the aim screen was seen; the overlay only shows right after.</summary>
        internal static int SeenFrame = -1;

        /// <summary>The top runs of the last board collected, for reviewing the player's own run
        /// against them after a finish. Kept even when the board's result came from the cache.</summary>
        internal static List<RunTrack> Loaded;

        /// <summary>Level and mutator <see cref="Loaded"/> belongs to.</summary>
        internal static int LoadedLevel = -1;
        internal static int LoadedMutator = -1;

        /// <summary>Index of the world record in <see cref="Loaded"/>.</summary>
        internal static int LoadedReference;

        private static bool cachedResult;
        private static int pendingLevel;
        private static int pendingMutator;

        internal static GameManager CurrentManager()
        {
            if (gameManagerField == null)
            {
                gameManagerField = AccessTools.Field(typeof(Hud), "gameManager");
            }

            return gameManagerField == null ? null : gameManagerField.GetValue(null) as GameManager;
        }

        /// <summary>A replay session as the analysis sees it.</summary>
        internal static RunTrack ToTrack(Replay.ReplaySession session, string name, int place, float finishTime)
        {
            Vector3[] positions = new Vector3[session.frameCount];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = session.frames[i / 60][i % 60].position;
            }

            List<RunEvent> events = new List<RunEvent>(session.eventCount);
            for (int i = 0; i < session.eventCount; i++)
            {
                Replay.ReplayEventRecord e = session.events[i / 100][i % 100];
                events.Add(new RunEvent { Type = e.type, Data = e.data, Time = e.timestamp });
            }

            return new RunTrack(name, place, Time.fixedDeltaTime, session.waitTime, positions, events, finishTime);
        }

        internal static void Tick()
        {
            GameManager manager = CurrentManager();
            if (manager == null || manager.gameplayState != GameManager.GameplayState.START_LEVEL_AIM)
            {
                return;
            }

            SeenFrame = Time.frameCount;

            if (Game.currentLevel < 0 || Game.isSpeedrun() || Game.isEndless()
                || Game.startedFrom != StartedFrom.DEFAULT)
            {
                return;
            }

            int mutator = LevelSelector.currentLevelMutator;
            LeaderboardCollection collection = LeaderboardsBackend.leaderboardForLevel(Game.currentLevel, mutator);
            if (collection == null || collection.leaderboards == null || collection.leaderboards.Length == 0)
            {
                return;
            }

            Leaderboard board = collection.leaderboards[0];
            if (board == null || board.count <= 0 || board.scores == null || board.responseTimestamp < 0f)
            {
                return;
            }

            int wanted = Mathf.Min(LimitConfig.TopCount.Value, Mathf.Min(board.count, board.scores.Length));
            if (wanted <= 0 || board.scores[0] == null)
            {
                return;
            }

            // The world record's replay handle changes whenever the record does, so it doubles as
            // the cache key: same key, same answer.
            string key = "v" + AnalysisVersion.ToString(Inv) + "_L" + Game.currentLevel.ToString(Inv) + "_m" + mutator.ToString(Inv) + "_" + board.scores[0].replayUGC.ToString(Inv);

            if (key != boardKey)
            {
                boardKey = key;
                pending = null;

                // A cached result is shown at once, but the runs are still fetched (from
                // SeumReplay's disk cache, usually) so a finished run can be reviewed against them.
                string cached = LimitCache.TryLoad(key);
                cachedResult = cached != null;
                pendingLevel = Game.currentLevel;
                pendingMutator = mutator;

                pending = new Score[wanted];
                for (int i = 0; i < wanted; i++)
                {
                    Score score = board.scores[i];
                    pending[i] = score;
                    if (score != null && score.replayUGC != ulong.MaxValue
                        && (score.replaySession == null || score.replaySessionUGC != score.replayUGC))
                    {
                        LeaderboardsBackend.downloadReplay(score);
                    }
                }

                requestedAt = Time.unscaledTime;
                Status = cachedResult ? cached : "Лимит: скачиваю топ-" + wanted + "…";
                StatusBoard = key;
            }

            if (pending == null)
            {
                return;
            }

            int ready = 0;
            foreach (Score score in pending)
            {
                if (score != null && score.replaySession != null && score.replaySessionUGC == score.replayUGC)
                {
                    ready++;
                }
            }

            bool timedOut = Time.unscaledTime - requestedAt > DownloadTimeoutSeconds;
            if (ready < pending.Length && !timedOut)
            {
                if (!cachedResult)
                {
                    Status = "Лимит: скачано " + ready + "/" + pending.Length + "…";
                }

                return;
            }

            Score[] scores = pending;
            pending = null;
            Analyse(key, scores);
        }

        private static void Analyse(string key, Score[] scores)
        {
            // The top runs as CSV are for working on the analysis outside the game, not for playing.
            bool saveCsv = LimitConfig.SaveRunCsv.Value;
            string dir = IOPath.Combine(Paths.CachePath, IOPath.Combine("SeumLimit", IOPath.Combine("runs", key)));
            if (saveCsv)
            {
                Directory.CreateDirectory(dir);
            }

            List<RunTrack> runs = new List<RunTrack>();
            int reference = -1;
            int referenceTime = int.MaxValue;

            foreach (Score score in scores)
            {
                if (score == null || score.replaySession == null || score.replaySessionUGC != score.replayUGC
                    || score.replaySession.frameCount < 2)
                {
                    continue;
                }

                Replay.ReplaySession session = score.replaySession;
                string stamp = "place" + score.place.ToString(Inv);

                if (saveCsv)
                {
                    StringBuilder info = new StringBuilder();
                    Dumper.WriteSession(info, session, score, dir, stamp);
                    File.WriteAllText(IOPath.Combine(dir, stamp + "_info.txt"), info.ToString(), Encoding.UTF8);
                }

                // Integer milliseconds, as the board has them: comparing against a float
                // reconstruction let a tied run further down take over as the reference.
                if (reference < 0 || score.time < referenceTime)
                {
                    reference = runs.Count;
                    referenceTime = score.time;
                }

                runs.Add(ToTrack(session, score.name, score.place, score.time / 1000f));
            }

            if (runs.Count == 0)
            {
                if (!cachedResult)
                {
                    Status = "Лимит: реплеи не скачались";
                    StatusBoard = key;
                }

                return;
            }

            Loaded = runs;
            LoadedLevel = pendingLevel;
            LoadedMutator = pendingMutator;
            LoadedReference = reference;

            if (cachedResult)
            {
                return;
            }

            SegmentLimit.Result result = SegmentLimit.Compute(runs, reference, SegmentLimit.Settings.Default);
            LevelProbe probe = LevelProbe.Create();
            PathLimit.Result path = PathLimit.Compute(runs[reference], PathLimit.Settings.Default,
                probe == null ? null : new Func<Vector3, Vector3, bool>(probe.IsWalkable),
                probe == null ? null : new Func<Vector3, Vector3, float>(probe.ClearDistance));
            string text = Describe(result, runs) + "\n" + DescribePath(path, probe != null);

            Plugin.Log.LogInfo("Board " + key + ":\n" + text);
            if (saveCsv)
            {
                File.WriteAllText(IOPath.Combine(dir, "result.txt"), text, Encoding.UTF8);
            }

            Status = text;
            StatusBoard = key;
            LimitCache.Store(key, text);
        }

        private static string DescribePath(PathLimit.Result path, bool probed)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Inv, "Лимит B {0:0.000} ({1:+0;-0;0} мс): разгон на земле −{2:0} мс, рор −{6:0} мс, телепорт −{7:0} мс ({8}), срезы −{4:0} мс ({5}); недобор скорости в воздухе {3:0} мс не засчитан",
                path.Limit, (path.Limit - path.ReferenceTime) * 1000f,
                path.GroundLoss * 1000f, path.AirLoss * 1000f, path.ShortcutGain * 1000f,
                probed ? path.Shortcuts.ToString(Inv) : "без уровня", path.RoarGain * 1000f,
                path.TeleportGain * 1000f, path.TeleportsChecked);

            int total = path.FreeTicks + path.LockedTicks + path.NeutralTicks;
            if (total > 0)
            {
                sb.AppendFormat(Inv, "\n  бег оценён на {0}% тиков, рор на {3}%; остальные паверапы ({1}%) и ожидание броска ({2}%) как у WR",
                    100 * path.FreeTicks / total, 100 * (path.LockedTicks - path.RoarTicks) / total,
                    100 * path.NeutralTicks / total, 100 * path.RoarTicks / total);
            }

            return sb.ToString();
        }

        private static string Describe(SegmentLimit.Result result, List<RunTrack> runs)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Inv, "WR {0:0.000} ({6})   Лимит A {1:0.000} ({2:+0;-0;0} мс)   топ-{3}, точек {4}, склеек {5}\n",
                result.ReferenceTime, result.Limit, (result.Limit - result.ReferenceTime) * 1000f,
                runs.Count, result.Checkpoints, result.Switches, runs[result.ReferenceRun].Name);

            // One line per stretch borrowed from another run, with what the whole stretch wins:
            // a borrowed run crosses the reference's checkpoints out of step, so its piece-by-piece
            // differences mean nothing on their own - only the stretch as a whole does.
            int i = 0;
            while (i < result.Segments.Count)
            {
                int run = result.Segments[i].Run;
                int j = i;
                float duration = 0f, referenceDuration = 0f;
                while (j < result.Segments.Count && result.Segments[j].Run == run)
                {
                    duration += result.Segments[j].Duration;
                    referenceDuration += result.Segments[j].ReferenceDuration;
                    j++;
                }

                // Stretches that win under half a millisecond are sub-tick noise, not a lesson.
                if (runs[run].Place != runs[result.ReferenceRun].Place && referenceDuration - duration >= 0.0005f)
                {
                    // Relative to the WR's own time over the same stretch: with tied records the
                    // stretch can come from a run with the same total, faster here and slower
                    // somewhere else.
                    sb.AppendFormat(Inv, "  {0:0.000}–{1:0.000}: этот отрезок как у #{2} {3}, на нём быстрее WR на {4:0} мс\n",
                        result.Segments[i].StartTime, result.Segments[j - 1].EndTime,
                        runs[run].Place, runs[run].Name, (referenceDuration - duration) * 1000f);
                }

                i = j;
            }

            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Finished results by board key (level, mutator, world record's replay handle). A result
    /// only goes stale when the record changes, and then the key changes with it.
    /// </summary>
    internal static class LimitCache
    {
        private static string Dir
        {
            get { return IOPath.Combine(Paths.CachePath, IOPath.Combine("SeumLimit", "results")); }
        }

        internal static string TryLoad(string key)
        {
            try
            {
                string path = IOPath.Combine(Dir, key + ".txt");
                return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not read cached limit " + key + ": " + e.Message);
                return null;
            }
        }

        internal static void Store(string key, string text)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(IOPath.Combine(Dir, key + ".txt"), text, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not cache limit " + key + ": " + e.Message);
            }
        }
    }
}
