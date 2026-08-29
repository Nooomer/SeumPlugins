using BepInEx.Configuration;
using UnityEngine;

namespace SeumReplay
{
    /// <summary>
    /// Everything here is about *reading* replays: unpacking them, fetching them from Steam and
    /// watching them. Nothing in this plugin touches recording or uploading, because the bytes a
    /// player uploads have to stay readable by an unmodded client.
    /// </summary>
    internal static class ReplayConfig
    {
        internal static ConfigEntry<bool> Enabled;

        // --- decoding ----------------------------------------------------------------------
        internal static ConfigEntry<bool> FixChunkTails;
        internal static ConfigEntry<bool> TolerateTruncated;

        // --- download ----------------------------------------------------------------------
        internal static ConfigEntry<bool> DiskCache;
        internal static ConfigEntry<int> CacheBudgetMegabytes;
        internal static ConfigEntry<int> CacheMaxAgeDays;
        internal static ConfigEntry<float> DownloadTimeout;
        internal static ConfigEntry<int> DownloadRetries;
        internal static ConfigEntry<int> PrefetchCount;

        // --- ui / playback ------------------------------------------------------------------
        internal static ConfigEntry<bool> StatusOverlay;
        internal static ConfigEntry<bool> PlaybackHotkeys;
        internal static ConfigEntry<KeyCode> PauseKey;
        internal static ConfigEntry<KeyCode> SlowerKey;
        internal static ConfigEntry<KeyCode> FasterKey;
        internal static ConfigEntry<KeyCode> StepKey;
        internal static ConfigEntry<KeyCode> RestartKey;

        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            FixChunkTails = cfg.Bind("02 - Decoding", "FixChunkTails", true,
                "The stock reader sizes the last chunk of every section with 'count % chunkSize', "
                + "so when the count is an exact multiple of the chunk size (60 frames, 100 events, "
                + "20 projectile entries) it reads that whole chunk as empty and every following "
                + "field is read from the wrong offset. That is the usual cause of a replay that "
                + "either sticks on 'Loading...' forever or plays back as garbage. Turn off only to "
                + "compare against stock behaviour.");

            TolerateTruncated = cfg.Bind("02 - Decoding", "TolerateTruncated", true,
                "If a replay blob is shorter than its own header claims, clamp the counts to what "
                + "actually arrived and play what there is instead of throwing. A short replay beats "
                + "no replay, and the stock code turns the exception into a permanent 'Loading...'.");

            DiskCache = cfg.Bind("03 - Download", "DiskCache", true,
                "Keep downloaded replays in BepInEx/cache/SeumReplay, keyed by their Steam UGC "
                + "handle. Opening the same replay again then costs a file read instead of a Steam "
                + "round trip. A new personal best gets a new handle, so the cache can never serve "
                + "a stale run.");

            CacheBudgetMegabytes = cfg.Bind("03 - Download", "CacheBudgetMegabytes", 64,
                new ConfigDescription(
                    "Upper bound for the on-disk replay cache. Oldest files are dropped first.",
                    new AcceptableValueRange<int>(1, 4096)));

            CacheMaxAgeDays = cfg.Bind("03 - Download", "CacheMaxAgeDays", 60,
                new ConfigDescription(
                    "Cached replays untouched for this long are deleted at startup. 0 disables the "
                    + "age check and leaves trimming to the size budget.",
                    new AcceptableValueRange<int>(0, 3650)));

            DownloadTimeout = cfg.Bind("03 - Download", "DownloadTimeout", 10f,
                new ConfigDescription(
                    "Seconds to wait for a Steam UGC download before the game gives up on it and "
                    + "re-issues it. Stock is 3, which is short enough that a slow connection keeps "
                    + "cancelling downloads that were about to finish. Applied at startup.",
                    new AcceptableValueRange<float>(1f, 120f)));

            DownloadRetries = cfg.Bind("03 - Download", "DownloadRetries", 5,
                new ConfigDescription(
                    "How many times a timed-out replay download is re-issued. Applied at startup.",
                    new AcceptableValueRange<int>(0, 50)));

            PrefetchCount = cfg.Bind("03 - Download", "PrefetchCount", 1,
                new ConfigDescription(
                    "While the level leaderboard is on screen and its replays are unlocked, start "
                    + "downloading the top N runs so that opening one is instant. 0 disables "
                    + "prefetching.",
                    new AcceptableValueRange<int>(0, 15)));

            StatusOverlay = cfg.Bind("04 - Interface", "StatusOverlay", true,
                "Replace the blind 'Loading...' with the actual state: download progress, which "
                + "attempt this is, and a retry hint when Steam gave up. While a replay is playing "
                + "the same line shows the playback position and speed.");

            PlaybackHotkeys = cfg.Bind("05 - Playback", "PlaybackHotkeys", true,
                "Keyboard control over replay playback, on top of the stock speed slider.");

            PauseKey = cfg.Bind("05 - Playback", "PauseKey", KeyCode.Space,
                "Pause / resume playback.");
            SlowerKey = cfg.Bind("05 - Playback", "SlowerKey", KeyCode.LeftBracket,
                "Halve playback speed (down to 0.05x).");
            FasterKey = cfg.Bind("05 - Playback", "FasterKey", KeyCode.RightBracket,
                "Double playback speed (up to 4x, the ceiling the game's own slider uses).");
            StepKey = cfg.Bind("05 - Playback", "StepKey", KeyCode.Period,
                "While paused, advance playback by one recorded frame.");
            RestartKey = cfg.Bind("05 - Playback", "RestartKey", KeyCode.Home,
                "Restart the replay from the beginning. When a replay failed to load, the same key "
                + "retries the download. Deliberately not R: the game's own restart action is read "
                + "in Hud.Update without checking the gameplay state, so R during a replay already "
                + "drops back to the aim screen.");

            VerboseLogging = cfg.Bind("06 - Diagnostics", "VerboseLogging", false,
                "Log every unpack and cache hit with sizes and timings.");
        }
    }
}
