using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Steamworks;

namespace SeumSteamOpt
{
    /// <summary>
    /// The two reasons SEUM talks to the leaderboard backend far more than it needs to.
    ///
    /// 1. Every read is two round trips instead of one. SteamDownloadRequest.start calls
    ///    FindOrCreateLeaderboard to turn a name like "level12_v8" into a SteamLeaderboard_t,
    ///    waits for the callback, and only then downloads. The handle is stable for the lifetime
    ///    of the process, so from the second read of a board onwards the lookup is pure overhead.
    ///
    /// 2. Every read happens on a blind 5 second timer. GameManager.Update re-requests the current
    ///    level's three boards, and SpeedrunSelector re-requests 30 boards - 90 download requests -
    ///    each time the timer fires, whether or not anything could have changed and whether or not
    ///    the previous batch has even come back.
    /// </summary>
    internal static class LeaderboardPatches
    {
        /// <summary>Leaderboard name to the handle Steam handed back for it.</summary>
        private static readonly Dictionary<string, SteamLeaderboard_t> Handles =
            new Dictionary<string, SteamLeaderboard_t>(128);

        /// <summary>Leaderboard key to the moment the game was last allowed to re-download it.</summary>
        private static readonly Dictionary<string, float> LastRequest =
            new Dictionary<string, float>(128);

        private static MethodInfo downloadLeaderboardName;
        private static MethodInfo downloadEntries;
        private static MethodInfo uploadLeaderboardName;

        private static bool handleCacheReady;

        internal static void Apply(Harmony harmony)
        {
            Type self = typeof(LeaderboardPatches);

            if (SteamOptConfig.CacheLeaderboardHandles.Value)
            {
                downloadLeaderboardName = AccessTools.Method(typeof(SteamDownloadRequest), "leaderboardName");
                downloadEntries = AccessTools.Method(typeof(SteamDownloadRequest), "downloadLeaderboardEntries");
                uploadLeaderboardName = AccessTools.Method(typeof(SteamUploadRequest), "leaderboardName");

                if (downloadLeaderboardName == null || downloadEntries == null)
                {
                    Plugin.Log.LogWarning(
                        "SteamDownloadRequest.leaderboardName/downloadLeaderboardEntries not found; "
                        + "leaderboard handle caching disabled.");
                }
                else
                {
                    handleCacheReady =
                        Patcher.Patch(harmony, self, typeof(SteamDownloadRequest), "start",
                            prefix: nameof(DownloadStartPrefix));

                    // Both request types learn handles. Uploads are rare, but they warm the cache
                    // for the three downloads a successful upload kicks off straight afterwards.
                    Patcher.Patch(harmony, self, typeof(SteamDownloadRequest), "leaderboardFoundOrCreated",
                        postfix: nameof(DownloadFoundPostfix));

                    if (uploadLeaderboardName != null)
                    {
                        Patcher.Patch(harmony, self, typeof(SteamUploadRequest), "leaderboardFoundOrCreated",
                            postfix: nameof(UploadFoundPostfix));
                    }
                }
            }

            if (SteamOptConfig.RefreshCooldownSeconds.Value > 0f)
            {
                Patcher.Patch(harmony, self, typeof(LeaderboardsSteamBackend), "downloadScore",
                    prefix: nameof(DownloadScorePrefix));
                Patcher.Patch(harmony, self, typeof(LeaderboardsSteamBackend), "downloadScoreWorkshop",
                    prefix: nameof(DownloadScoreWorkshopPrefix));
            }
        }

        // ----------------------------------------------------------------------- handle cache

        /// <summary>
        /// Skips FindOrCreateLeaderboard and goes straight to the download when the handle for this
        /// board is already known. On a miss the original runs untouched and teaches the cache
        /// through <see cref="DownloadFoundPostfix"/>.
        /// </summary>
        private static bool DownloadStartPrefix(SteamDownloadRequest __instance)
        {
            if (!handleCacheReady)
            {
                return true;
            }

            string name;
            SteamLeaderboard_t handle;
            try
            {
                name = downloadLeaderboardName.Invoke(__instance, null) as string;
            }
            catch (Exception e)
            {
                // Nothing has been sent yet, so handing the request back to the original is free.
                Plugin.Log.LogWarning("leaderboard handle cache failed, reverting to vanilla: " + e.Message);
                handleCacheReady = false;
                return true;
            }

            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            lock (Handles)
            {
                if (!Handles.TryGetValue(name, out handle))
                {
                    return true;
                }
            }

            try
            {
                downloadEntries.Invoke(__instance, new object[] { handle });
            }
            catch (Exception e)
            {
                // The download may already be in flight, so running the original as well would
                // duplicate it. Retire this request instead - marking it finished is what the
                // game's own failure path does, and it drops out of the queue on the next update.
                Plugin.Log.LogWarning("leaderboard download from cached handle failed, reverting to "
                    + "vanilla for later requests: " + e.Message);
                handleCacheReady = false;
                __instance.finished = true;
                return false;
            }

            Counters.Add(ref Counters.LeaderboardFinds, 1);
            return false;
        }

        private static void DownloadFoundPostfix(SteamDownloadRequest __instance,
            LeaderboardFindResult_t callback, bool failure)
        {
            Remember(downloadLeaderboardName, __instance, callback, failure);
        }

        private static void UploadFoundPostfix(SteamUploadRequest __instance,
            LeaderboardFindResult_t callback, bool failure)
        {
            Remember(uploadLeaderboardName, __instance, callback, failure);
        }

        private static void Remember(MethodInfo nameMethod, object request,
            LeaderboardFindResult_t callback, bool failure)
        {
            if (nameMethod == null || failure || callback.m_bLeaderboardFound == 0)
            {
                return;
            }

            try
            {
                string name = nameMethod.Invoke(request, null) as string;
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                lock (Handles)
                {
                    Handles[name] = callback.m_hSteamLeaderboard;
                }

                if (SteamOptConfig.VerboseLogging.Value)
                {
                    Plugin.Log.LogInfo("leaderboard handle cached: " + name);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("could not cache leaderboard handle: " + e.Message);
            }
        }

        // ------------------------------------------------------------------- refresh cooldown

        private static bool DownloadScorePrefix(ulong leaderboardId, string leaderboardPrefix)
        {
            return AllowRefresh(leaderboardPrefix + "|" + leaderboardId);
        }

        private static bool DownloadScoreWorkshopPrefix(ulong leaderboardId, ulong leaderboardRev,
            string leaderboardPrefix)
        {
            return AllowRefresh(leaderboardPrefix + "|" + leaderboardId + "|" + leaderboardRev);
        }

        /// <summary>
        /// One downloadScore call queues three requests (global, around-user, friends), so a blocked
        /// refresh is three downloads that never happen. Blocking it leaves the previously
        /// downloaded scores on screen: nothing in the game reads Leaderboard.requestTimestamp or
        /// responseTimestamp, so there is no "loading" state left hanging either.
        ///
        /// The upload path is deliberately not covered - a successful score upload adds its own
        /// SteamDownloadRequests directly instead of going through downloadScore, so a fresh
        /// personal best still appears immediately.
        /// </summary>
        private static bool AllowRefresh(string key)
        {
            float cooldown = SteamOptConfig.RefreshCooldownSeconds.Value;
            if (cooldown <= 0f)
            {
                return true;
            }

            float now = Clock.Now;
            lock (LastRequest)
            {
                float last;
                if (LastRequest.TryGetValue(key, out last) && now - last < cooldown)
                {
                    Counters.Add(ref Counters.LeaderboardDownloads, 3);
                    return false;
                }

                LastRequest[key] = now;
            }

            return true;
        }
    }
}
