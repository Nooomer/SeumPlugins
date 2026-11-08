using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumLeaderboardFilter
{
    /// <summary>
    /// Hides the #1 entry of a Steam leaderboard when its author is in a local
    /// SteamID blacklist.
    ///
    /// Why only #1: on the boards this was built for, the storage/cheated scores
    /// are the ones at the top. Lower places are a noisy mix of legitimate runs
    /// and old data, and hiding them would risk removing a legitimate result for
    /// no practical gain. The blacklist is deliberately manual — there is no
    /// automatic "too fast" rule, because legitimate #1 times vary enormously
    /// between boards (e.g. legitimate Roar results start at 0.500 s on Hill).
    ///
    /// The patch runs inside SteamDownloadRequest.leaderboardScoresDownloaded,
    /// before LeaderboardsSteamBackend.update copies the entries into
    /// Leaderboard.scores, so every screen (level selector, speedrun selector,
    /// HUD, replay list) sees the same filtered board. Score.place is left
    /// untouched: hiding #1 simply removes that row, #2 keeps its real rank.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            FilterConfig.Bind(Config);

            if (!FilterConfig.Enabled.Value)
            {
                Log.LogInfo("SeumLeaderboardFilter is disabled by config.");
                return;
            }

            Blacklist.Initialize(FilterConfig.BlacklistFile.Value);
            Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(DownloadRangePatch));
            harmony.PatchAll(typeof(DownloadBufferPatch));
            harmony.PatchAll(typeof(LeaderboardFilterPatch));

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded; "
                + $"{Blacklist.Count} blacklisted player(s), blacklist: {Blacklist.Path}");
        }
    }
}
