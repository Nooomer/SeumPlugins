using BepInEx.Configuration;

namespace SeumSteamOpt
{
    /// <summary>
    /// Every switch here defaults to "on" because none of them change what the player sees - they
    /// only stop the game asking Steam a question it already knows the answer to. The one entry
    /// that does have a visible effect is <see cref="RefreshCooldownSeconds"/>: it decides how
    /// stale a leaderboard is allowed to get, so it is a number rather than a checkbox.
    /// </summary>
    internal static class SteamOptConfig
    {
        internal static ConfigEntry<bool> Enabled;

        // --- leaderboards ------------------------------------------------------------------
        internal static ConfigEntry<bool> CacheLeaderboardHandles;
        internal static ConfigEntry<float> RefreshCooldownSeconds;

        // --- friends -----------------------------------------------------------------------
        internal static ConfigEntry<bool> CachePersonaNames;
        internal static ConfigEntry<float> UserInfoRequestInterval;

        // --- achievements ------------------------------------------------------------------
        internal static ConfigEntry<bool> CacheUnlockedAchievements;

        // --- workshop ----------------------------------------------------------------------
        internal static ConfigEntry<float> ItemStateCacheSeconds;

        // --- diagnostics -------------------------------------------------------------------
        internal static ConfigEntry<float> StatsLogInterval;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            CacheLeaderboardHandles = cfg.Bind("02 - Leaderboards", "CacheLeaderboardHandles", true,
                "Every leaderboard download starts with FindOrCreateLeaderboard to turn a leaderboard "
                + "name into a handle, even though the game asks for the same few hundred names over "
                + "and over and the handle never changes while the process is running. Caching it "
                + "removes one Steam round trip from every single leaderboard read - half of all "
                + "leaderboard traffic after the first look at a given board.");

            RefreshCooldownSeconds = cfg.Bind("02 - Leaderboards", "RefreshCooldownSeconds", 30f,
                new ConfigDescription(
                    "Minimum time between two downloads of the same leaderboard. Vanilla re-downloads "
                    + "on a fixed 5 second timer with no regard for how many boards that is: the "
                    + "speedrun selector alone refreshes 30 boards (90 requests) every 5 seconds, and "
                    + "an in-level HUD refreshes 3. Your own new record is still shown immediately - "
                    + "a successful upload re-downloads directly and does not go through this "
                    + "cooldown. Set to 0 to keep vanilla behaviour.",
                    new AcceptableValueRange<float>(0f, 600f)));

            CachePersonaNames = cfg.Bind("03 - Friends", "CachePersonaNames", true,
                "While a downloaded leaderboard still has unresolved players on it, the game asks "
                + "Steam for every row's display name on every frame - 15 names per board, and the "
                + "level selector has dozens of boards in flight at once. Names are cached per Steam "
                + "id and dropped again when Steam reports that the user's persona changed, so what "
                + "is displayed stays correct.");

            UserInfoRequestInterval = cfg.Bind("03 - Friends", "UserInfoRequestInterval", 0.25f,
                new ConfigDescription(
                    "The same loop also calls RequestUserInformation for every row on every frame. "
                    + "The answer only changes when Steam has finished fetching the user, so asking "
                    + "60 times a second is pointless; this is how long a previous answer is reused. "
                    + "Set to 0 to keep vanilla behaviour.",
                    new AcceptableValueRange<float>(0f, 5f)));

            CacheUnlockedAchievements = cfg.Bind("04 - Achievements", "CacheUnlockedAchievements", true,
                "saveAchievement reads the achievement back from Steam before deciding to do nothing. "
                + "Two of the call sites are per-fireball and per-jump, so past 666 shots or jumps "
                + "that is a marshalled Steam call several times a second for an achievement that is "
                + "already unlocked. Confirmed unlocks are remembered for the session.");

            ItemStateCacheSeconds = cfg.Bind("05 - Workshop", "ItemStateCacheSeconds", 5f,
                new ConfigDescription(
                    "The workshop screen polls GetItemState once a second for every subscribed map. "
                    + "The result only changes when Steam raises an install / update / subscription "
                    + "callback, and those clear this cache immediately, so polling faster than this "
                    + "buys nothing. Set to 0 to keep vanilla behaviour.",
                    new AcceptableValueRange<float>(0f, 60f)));

            StatsLogInterval = cfg.Bind("06 - Diagnostics", "StatsLogInterval", 0f,
                new ConfigDescription(
                    "Seconds between 'calls avoided so far' lines in the BepInEx log. 0 logs only "
                    + "once, when the game shuts down.",
                    new AcceptableValueRange<float>(0f, 3600f)));

            VerboseLogging = cfg.Bind("06 - Diagnostics", "VerboseLogging", false,
                "Log each patch as it is applied, and every leaderboard handle as it is learned.");
        }
    }
}
