using System;
using HarmonyLib;
using UnityEngine;

namespace SeumSteamOpt
{
    /// <summary>
    /// SpeedrunSelector.refreshLeaderboards re-downloads every leaderboard the screen could possibly
    /// need: full game and each of the 14 zones, each in a with-beers and a without-beers variant.
    /// That is 30 boards, 90 SteamDownloadRequests, every 5 seconds for as long as the screen is up.
    ///
    /// The screen draws exactly one of them. The last line of SpeedrunSelector.draw is
    /// Hud.drawScores(Speedrun.getLeaderboardCollection(), ...), and that collection is the board for
    /// the currently selected mode - the same board Speedrun.refreshCurrentLeaderboards refreshes.
    /// The other 29 are downloaded and never read by anything.
    ///
    /// So the fix is not to slow the timer down but to stop fetching what nobody looks at. The
    /// selected board keeps the vanilla 5 second cadence, and a selection change refreshes at once,
    /// which is what the vanilla prefetch was standing in for.
    /// </summary>
    internal static class SpeedrunSelectorPatches
    {
        private static MonoBehaviour lastBehaviour;

        private static bool haveSelection;
        private static bool lastFullGame;
        private static int lastZone;
        private static bool lastBeers;
        private static int lastMutator;

        internal static void Apply(Harmony harmony)
        {
            if (!SteamOptConfig.RefreshOnlyVisibleSpeedrunBoard.Value)
            {
                return;
            }

            Type self = typeof(SpeedrunSelectorPatches);

            if (!Patcher.Patch(harmony, self, typeof(SpeedrunSelector), "refreshLeaderboards",
                    prefix: nameof(RefreshLeaderboardsPrefix)))
            {
                return;
            }

            // Without this the selected board would only catch up on the next 5 second tick, which
            // would be a visible regression against the vanilla prefetch.
            Patcher.Patch(harmony, self, typeof(SpeedrunSelector), "draw",
                postfix: nameof(DrawPostfix));
        }

        private static bool RefreshLeaderboardsPrefix(MonoBehaviour behaviour)
        {
            lastBehaviour = behaviour;
            Refresh(behaviour);
            Counters.Add(ref Counters.LeaderboardDownloads, SkippedRequests());
            return false;
        }

        /// <summary>
        /// draw() runs once per IMGUI event, so this has to stay a handful of field reads. It only
        /// does anything on the event where the player actually moved the selection.
        /// </summary>
        private static void DrawPostfix()
        {
            Speedrun.SpeedrunSettings settings = Speedrun.settings;
            if (settings == null)
            {
                return;
            }

            if (haveSelection
                && settings.fullGame == lastFullGame
                && settings.zone == lastZone
                && settings.beersRequired == lastBeers
                && settings.mutator == lastMutator)
            {
                return;
            }

            bool first = !haveSelection;
            Remember(settings);

            // On the very first draw the periodic refresh has already run this frame; refreshing
            // again would just be a duplicate.
            if (!first)
            {
                Refresh(lastBehaviour);
            }
        }

        private static void Refresh(MonoBehaviour behaviour)
        {
            try
            {
                Speedrun.refreshCurrentLeaderboards(behaviour);
                Remember(Speedrun.settings);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("speedrun leaderboard refresh failed: " + e.Message);
            }
        }

        private static void Remember(Speedrun.SpeedrunSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            lastFullGame = settings.fullGame;
            lastZone = settings.zone;
            lastBeers = settings.beersRequired;
            lastMutator = settings.mutator;
            haveSelection = true;
        }

        /// <summary>
        /// How many SteamDownloadRequests the skipped batch would have queued, counted from the same
        /// arrays the original walks so the number stays right if the level layout ever changes.
        /// </summary>
        private static long SkippedRequests()
        {
            try
            {
                int boards = 2 + LevelSelector.levelMapping.Length * 2 + LevelSelector.epLevels.Length * 2;
                foreach (int[][] dlc in LevelSelector.dlcLevels)
                {
                    boards += dlc.Length * 2;
                }

                // One board is still refreshed; each board is three requests (global, around, friends).
                return (boards - 1) * 3;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
