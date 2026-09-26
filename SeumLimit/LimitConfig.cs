using BepInEx.Configuration;
using UnityEngine;

namespace SeumLimit
{
    /// <summary>
    /// Settings read from BepInEx/config/SeumLimit.cfg. F7 is the one function key the other
    /// plugins in this repository have not taken yet.
    /// </summary>
    internal static class LimitConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<KeyCode> DumpKey;
        internal static ConfigEntry<int> TopCount;
        internal static ConfigEntry<bool> ShowOverlay;
        internal static ConfigEntry<bool> RecordLive;
        internal static ConfigEntry<bool> SaveRunCsv;
        internal static ConfigEntry<int> KeepReviews;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch.");

            TopCount = cfg.Bind("01 - General", "TopCount", 5, new ConfigDescription(
                "How many of the top runs to download and splice together for the limit.",
                new AcceptableValueRange<int>(1, 15)));

            ShowOverlay = cfg.Bind("01 - General", "ShowOverlay", true,
                "Show the limit in the corner of the aim screen.");

            KeepReviews = cfg.Bind("01 - General", "KeepReviews", 50, new ConfigDescription(
                "How many run reviews to keep in BepInEx/cache/SeumLimit/reviews; older ones are "
                + "deleted at startup. 0 stops saving them (the review is still shown).",
                new AcceptableValueRange<int>(0, 10000)));

            RecordLive = cfg.Bind("02 - Diagnostics", "RecordLive", false,
                "Record every run you play, tick by tick, with inputs and the character's internal "
                + "state, to BepInEx/cache/SeumLimit/live. For working on the powerup models only: "
                + "it writes a few hundred KB per attempt, restarts included.");

            SaveRunCsv = cfg.Bind("02 - Diagnostics", "SaveRunCsv", false,
                "Also save the downloaded top runs as CSV under BepInEx/cache/SeumLimit/runs, for "
                + "working on the analysis outside the game.");

            DumpKey = cfg.Bind("02 - Diagnostics", "DumpKey", KeyCode.F7,
                "Writes a diagnostic dump to BepInEx/cache/SeumLimit/dump: the physics step, the "
                + "character's movement settings, the level's colliders grouped by how they are "
                + "marked, and - if a replay is loaded - that replay as CSV. Press it on a level, "
                + "and again while a leaderboard replay is playing.");
        }
    }
}
