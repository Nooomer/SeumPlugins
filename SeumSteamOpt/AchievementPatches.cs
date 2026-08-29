using System;
using System.Collections.Generic;
using HarmonyLib;
using Steamworks;

namespace SeumSteamOpt
{
    /// <summary>
    /// SeumSteam.saveAchievement always reads the achievement back from Steam before deciding that
    /// there is nothing to do. Most call sites are one-off events where that costs nothing, but
    /// "Fire666times" runs on every fireball and "Jump666times" on every jump, so once those
    /// counters are past 666 the game marshals a Steam call several times a second for an
    /// achievement it already owns.
    ///
    /// An unlocked achievement never becomes locked again while the game is running, so a confirmed
    /// unlock is remembered and the call is dropped entirely from then on.
    /// </summary>
    internal static class AchievementPatches
    {
        private static readonly HashSet<string> Unlocked = new HashSet<string>(StringComparer.Ordinal);

        internal static void Apply(Harmony harmony)
        {
            if (!SteamOptConfig.CacheUnlockedAchievements.Value)
            {
                return;
            }

            Patcher.Patch(harmony, typeof(AchievementPatches), typeof(SeumSteam), "saveAchievement",
                prefix: nameof(SaveAchievementPrefix),
                postfix: nameof(SaveAchievementPostfix));
        }

        private static bool SaveAchievementPrefix(string achievement, out bool __state)
        {
            // Harmony runs the postfix even when this prefix skips the original. Without this flag
            // the confirmation read below would fire on every skipped call, which is exactly the
            // Steam call the cache exists to avoid.
            __state = true;

            if (achievement == null)
            {
                return true;
            }

            lock (Unlocked)
            {
                if (!Unlocked.Contains(achievement))
                {
                    return true;
                }
            }

            Counters.Add(ref Counters.AchievementReads, 1);
            __state = false;
            return false;
        }

        /// <summary>
        /// Confirms with Steam rather than assuming the original succeeded: SetAchievement and
        /// StoreStats can fail while Steam is offline, and caching an unlock that never happened
        /// would silently cost the player the achievement. This costs one extra read per achievement
        /// per session, against one saved on every later call.
        /// </summary>
        private static void SaveAchievementPostfix(string achievement, bool __state)
        {
            if (!__state || achievement == null || !SteamState.Initialized)
            {
                return;
            }

            try
            {
                bool achieved;
                if (!SteamUserStats.GetAchievement(achievement, out achieved) || !achieved)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            lock (Unlocked)
            {
                Unlocked.Add(achievement);
            }

            if (SteamOptConfig.VerboseLogging.Value)
            {
                Plugin.Log.LogInfo("achievement confirmed unlocked, no longer queried: " + achievement);
            }
        }
    }
}
