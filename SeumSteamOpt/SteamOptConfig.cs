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
        internal static ConfigEntry<bool> RefreshOnlyVisibleSpeedrunBoard;
        internal static ConfigEntry<bool> SkipDuplicateInFlightRequests;
        internal static ConfigEntry<float> RefreshCooldownSeconds;

        // --- friends -----------------------------------------------------------------------
        internal static ConfigEntry<bool> CachePersonaNames;
        internal static ConfigEntry<float> UserInfoRequestInterval;

        // --- achievements ------------------------------------------------------------------
        internal static ConfigEntry<bool> CacheUnlockedAchievements;

        // --- workshop ----------------------------------------------------------------------
        internal static ConfigEntry<float> ItemStateCacheSeconds;
        internal static ConfigEntry<bool> DeferWorkshopStartup;
        internal static ConfigEntry<bool> SkipEmptyUserMapQuery;

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

            RefreshOnlyVisibleSpeedrunBoard = cfg.Bind("02 - Leaderboards", "RefreshOnlyVisibleSpeedrunBoard", true,
                "The speedrun selector re-downloads 30 leaderboards every 5 seconds - full game and "
                + "every zone, each in a with-beers and a without-beers variant - but the screen only "
                + "ever draws one of them, the one for the currently selected mode. The other 29 are "
                + "fetched and never read. This refreshes the selected board only, on the same 5 "
                + "second timer, plus immediately whenever the selection changes, so what is on "
                + "screen is exactly as fresh as vanilla and 29 boards of traffic disappear.");

            SkipDuplicateInFlightRequests = cfg.Bind("02 - Leaderboards", "SkipDuplicateInFlightRequests", true,
                "Drops a leaderboard download when an identical one is still on the wire - the game "
                + "queues the same board again on every zone change and every level start without "
                + "checking whether the previous batch has come back. This cannot make anything "
                + "staler, because the answer it would have waited for is already coming, so unlike "
                + "RefreshCooldownSeconds it costs no freshness at all.");

            RefreshCooldownSeconds = cfg.Bind("02 - Leaderboards", "RefreshCooldownSeconds", 0f,
                new ConfigDescription(
                    "Extra minimum time between two downloads of the same leaderboard, on top of the "
                    + "in-flight dedup above. Off by default because it is the one setting here that "
                    + "genuinely delays data: at 30, a rival's new score can take up to 30 seconds to "
                    + "appear. Keep it below the game's own 5 second refresh timer or it will start "
                    + "eating scheduled refreshes as the two timers drift in and out of phase. It is "
                    + "cleared on every scene load, so a mod that applies a setting by reloading the "
                    + "level - VelocityMeter's leaderboard range editor does exactly that - still gets "
                    + "its fresh download. Your own new record is never affected either: a successful "
                    + "upload re-downloads directly, bypassing this path.",
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

            DeferWorkshopStartup = cfg.Bind("05 - Workshop", "DeferWorkshopStartup", true,
                "Entering the main menu loads the whole workshop library whether or not you open the "
                + "workshop screen: a UGC details query for every subscribed map, and then, per map, "
                + "an HTTP download of its preview image plus a GetUserItemVote and a "
                + "GetAppDependencies call. Nothing outside the workshop screen reads any of it. This "
                + "holds that work back until the screen is actually opened. The cost is a short load "
                + "the first time you open the workshop in a session.");

            SkipEmptyUserMapQuery = cfg.Bind("05 - Workshop", "SkipEmptyUserMapQuery", true,
                "checkIfUserMapsArePublished runs on every return to the main menu and sends a UGC "
                + "query built from the maps you authored locally. If you have not authored any - "
                + "which is the case for everyone who does not use the level editor - the query "
                + "carries zero ids and can never return anything. This skips it.");

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
