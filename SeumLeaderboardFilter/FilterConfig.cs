using BepInEx.Configuration;

namespace SeumLeaderboardFilter
{
    internal static class FilterConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> DryRun;
        internal static ConfigEntry<bool> LogHidden;
        internal static ConfigEntry<bool> NeverHideSelf;
        internal static ConfigEntry<bool> RenumberGlobal;

        internal static ConfigEntry<bool> HidePortalStorage;
        internal static ConfigEntry<int> PortalMaxMs;
        internal static ConfigEntry<float> PortalRatio;

        internal static ConfigEntry<bool> HideTopGap;
        internal static ConfigEntry<float> TopGapPercent;
        internal static ConfigEntry<int> TopGapMinMs;
        internal static ConfigEntry<bool> TopGapChain;

        internal static ConfigEntry<string> BlacklistFile;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind(
                "01 - General",
                "Enabled",
                true,
                "Master switch. When false no Harmony patch is applied at all.");

            DryRun = cfg.Bind(
                "01 - General",
                "DryRun",
                false,
                "Log every entry that would be hidden, but leave the leaderboard untouched. "
                + "Use this to audit both the blacklist and the portal-storage rule.");

            LogHidden = cfg.Bind(
                "01 - General",
                "LogHidden",
                true,
                "Write one line per hidden score to the BepInEx log.");

            NeverHideSelf = cfg.Bind(
                "01 - General",
                "NeverHideSelf",
                true,
                "Never hide the local player's own scores, even if their SteamID is blacklisted.");

            RenumberGlobal = cfg.Bind(
                "01 - General",
                "RenumberGlobal",
                true,
                "On the global board, after rows are hidden, renumber the remaining rows 1..N "
                + "instead of leaving gaps (hiding #1 makes the next visible row #1). Only "
                + "applies when the download actually starts at rank 1; around-user and friends "
                + "lists keep their real global ranks, as does a custom VelocityMeter range.");

            HidePortalStorage = cfg.Bind(
                "02 - Rules",
                "HidePortalStorage",
                true,
                "Hide portal-storage outliers on the global board: a score at most PortalMaxMs "
                + "that is at least PortalRatio times faster than the next distinct time. This is "
                + "checked per row, not per player. It catches 0.500 against 1.049 without "
                + "touching legitimate short levels (Hill RR 0.500 -> 0.516). A whole-cluster rule "
                + "is deliberately not used: on Hill/Easy Dizzy it would delete dozens of "
                + "legitimate 500-600 ms results.");

            PortalMaxMs = cfg.Bind(
                "02 - Rules",
                "PortalMaxMs",
                600,
                "Upper bound, in milliseconds, for the portal-storage rule.");

            PortalRatio = cfg.Bind(
                "02 - Rules",
                "PortalRatio",
                2.0f,
                "The next distinct score must be at least this many times larger than the "
                + "candidate for it to be treated as portal storage. 2.0 means the candidate "
                + "is at least twice as fast as the next result in the download.");

            HideTopGap = cfg.Bind(
                "02 - Rules",
                "HideTopGap",
                true,
                "Hide a soft power-up outlier at the very top of the global board: the result "
                + "must be both TopGapPercent faster and TopGapMinMs faster than the next "
                + "distinct time. This catches 1.924 vs 4.493 (57%, 2569 ms) and 6.171 vs 7.556 "
                + "(18.3%, 1385 ms), while Pyramid RR 10.783 -> 11.600 (7%, 817 ms), Dominoes "
                + "2.783 -> 3.016 (7.7%, 233 ms) and Satan RR 21.516 -> 22.000 (2.2%, 484 ms) "
                + "stay. Raise the percentage if it starts touching legitimate records.");

            TopGapPercent = cfg.Bind(
                "02 - Rules",
                "TopGapPercent",
                0.18f,
                "Relative part of the soft top-gap rule: the next distinct time must be at "
                + "least this fraction larger than the candidate. 0.18 = 18%.");

            TopGapMinMs = cfg.Bind(
                "02 - Rules",
                "TopGapMinMs",
                1000,
                "Absolute part of the soft top-gap rule, in milliseconds. A short board with "
                + "a small absolute gap is never touched, even if the percentage looks large.");

            TopGapChain = cfg.Bind(
                "02 - Rules",
                "TopGapChain",
                true,
                "If the top result qualifies, also check the next one against the one after it, "
                + "and so on, until the chain stops. This removes 1.924 and 4.493 together when "
                + "both are far below the 5.55 cluster, but it never jumps over the first clean "
                + "result.");

            BlacklistFile = cfg.Bind(
                "03 - Blacklist",
                "BlacklistFile",
                "SeumLeaderboardFilter.blacklist.txt",
                "Path to the local player blacklist, relative to the BepInEx/config folder unless "
                + "absolute. The file is created with the known cheaters on first launch; edit it "
                + "and the plugin picks up changes on the next leaderboard download, no restart "
                + "needed. Every score of a listed player is hidden, at any place.");
        }
    }
}
