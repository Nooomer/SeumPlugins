using System.Collections.Generic;
using HarmonyLib;
using Steamworks;

namespace SeumLeaderboardFilter
{
    /// <summary>
    /// Runs after Steam has filled a download request, but before
    /// LeaderboardsSteamBackend.update copies the entries into the visible
    /// Leaderboard.
    ///
    /// Three independent reasons to drop a row:
    ///
    ///  - the author is in the local player blacklist — all of their rows go,
    ///    at any place, so keaz #1 + bruh #2 leaves neither on the board;
    ///  - the row is a portal storage on the global board: at most 600 ms and
    ///    at least twice as fast as the next distinct time. This is checked
    ///    per row, not per player: a non-blacklisted author loses exactly the
    ///    rows that satisfy the rule, not their whole board. In a 500/533/1016
    ///    cluster the 500 goes and the 533 may stay, because for the 533 the
    ///    next distinct time is 1016 (less than 2x). Removing the whole cluster
    ///    was tried and is not safe: legitimate Hill/Easy Dizzy boards contain
    ///    long 500-600 clusters followed by slower results, and a cluster rule
    ///    deletes all of them;
    ///  - a soft power-up gap at the top of the global board: the result is at
    ///    least 1000 ms and 18% faster than the next distinct time. Only the
    ///    leading chain is checked: Pyramid 10.783 -> 11.600 (817 ms, 7%) is
    ///    untouched, while 1.924 -> 4.493 (2569 ms, 57%) and 6.171 -> 7.556
    ///    (1385 ms, 18.3%) are removed. The chain stops at the first result that
    ///    does not qualify, so a legitimate #2 is never dragged down by a
    ///    suspicious #1.
    ///
    /// Score.place is renumbered on the global board when RenumberGlobal is on
    /// and the download starts at rank 1, so hiding #1 makes the next visible
    /// row #1. Around-user and friends downloads, and a custom VelocityMeter
    /// range that does not start at 1, keep their real global ranks.
    /// </summary>
    [HarmonyPatch(typeof(SteamDownloadRequest), "leaderboardScoresDownloaded")]
    internal static class LeaderboardFilterPatch
    {
        private static readonly HashSet<string> Logged = new HashSet<string>();

        private static void Postfix(SteamDownloadRequest __instance)
        {
            if (__instance == null)
            {
                return;
            }

            if (!FilterConfig.Enabled.Value)
            {
                // The download-buffer patches may still be applied if the user
                // turned the plugin off at runtime. Never leave a scoreCount
                // larger than the 15-element Leaderboard.scores array behind.
                if (__instance.scoreCount > 15)
                {
                    __instance.scoreCount = 15;
                }

                return;
            }

            Blacklist.RefreshIfChanged();

            SteamScore[] scores = __instance.downloadedScores;
            int count = __instance.scoreCount;
            if (scores == null || count <= 0 || count > scores.Length)
            {
                return;
            }

            int[] nextDistinct = BuildNextDistinct(scores, count);
            bool globalBoard = __instance.type == 0;
            bool renumber = globalBoard
                && FilterConfig.RenumberGlobal.Value
                && scores[0].place == 1;
            bool topGapChain = globalBoard && FilterConfig.HideTopGap.Value;
            int kept = 0;

            for (int i = 0; i < count; i++)
            {
                SteamScore score = scores[i];
                string reason = ReasonToHide(score, nextDistinct[i], globalBoard);

                if (reason == null && topGapChain)
                {
                    if (IsTopGap(score, nextDistinct[i]))
                    {
                        reason = "top-gap";
                    }
                    else
                    {
                        topGapChain = false;
                    }
                }
                else if (reason != null)
                {
                    topGapChain = false;
                }

                if (reason == null)
                {
                    scores[kept++] = score;
                    continue;
                }

                LogHidden(__instance, score, reason);

                if (FilterConfig.DryRun.Value)
                {
                    scores[kept++] = score;
                }
            }

            for (int i = kept; i < count; i++)
            {
                scores[i] = default(SteamScore);
            }

            // The shipped update() copies exactly scoreCount entries into
            // Leaderboard.scores, and that array is only 15 elements long. The
            // extra entries were only needed as a replacement pool, so hand
            // update() just the visible window.
            int visible = kept < 15 ? kept : 15;
            __instance.scoreCount = visible;

            if (renumber && !FilterConfig.DryRun.Value)
            {
                for (int i = 0; i < visible; i++)
                {
                    scores[i].place = i + 1;
                }
            }
        }

        private static string ReasonToHide(SteamScore score, int next, bool globalBoard)
        {
            ulong steamId = score.id.m_SteamID;

            if (FilterConfig.NeverHideSelf.Value && IsLocalPlayer(steamId))
            {
                return null;
            }

            if (Blacklist.Contains(steamId))
            {
                return "player-blacklist";
            }

            if (globalBoard
                && FilterConfig.HidePortalStorage.Value
                && score.time <= FilterConfig.PortalMaxMs.Value
                && next >= score.time * FilterConfig.PortalRatio.Value)
            {
                return "portal-storage";
            }

            return null;
        }

        private static bool IsTopGap(SteamScore score, int next)
        {
            if (next <= 0 || next <= score.time)
            {
                return false;
            }

            if (FilterConfig.NeverHideSelf.Value && IsLocalPlayer(score.id.m_SteamID))
            {
                return false;
            }

            int gap = next - score.time;
            return gap >= FilterConfig.TopGapMinMs.Value
                && gap >= next * FilterConfig.TopGapPercent.Value;
        }

        /// <summary>
        /// Smallest time strictly greater than each entry's time, within this
        /// download. 0 means there is no later result in the list.
        /// </summary>
        private static int[] BuildNextDistinct(SteamScore[] scores, int count)
        {
            var next = new int[count];
            for (int i = 0; i < count; i++)
            {
                int best = 0;
                for (int j = 0; j < count; j++)
                {
                    int candidate = scores[j].time;
                    if (candidate > scores[i].time && (best == 0 || candidate < best))
                    {
                        best = candidate;
                    }
                }

                next[i] = best;
            }

            return next;
        }

        private static void LogHidden(SteamDownloadRequest request, SteamScore score, string reason)
        {
            if (!FilterConfig.LogHidden.Value && !FilterConfig.DryRun.Value)
            {
                return;
            }

            string board = request.leaderboardPrefix + request.leaderboardId + "_v8";
            string key = board + "|" + score.id.m_SteamID + "|" + score.time + "|" + reason;
            if (!Logged.Add(key))
            {
                return;
            }

            if (Logged.Count > 4096)
            {
                Logged.Clear();
                Logged.Add(key);
            }

            string suffix = FilterConfig.DryRun.Value ? " (dry run, kept)" : "";
            Plugin.Log.LogInfo(
                $"Hid {reason} on {board}: place {score.place}, {score.time} ms, "
                + $"SteamID {score.id.m_SteamID}{suffix}");
        }

        private static bool IsLocalPlayer(ulong steamId)
        {
            try
            {
                return SteamUser.GetSteamID().m_SteamID == steamId;
            }
            catch
            {
                // Steam should be initialized inside this callback, but a failed
                // lookup must not make the plugin hide the local player by accident.
                return false;
            }
        }
    }
}
