using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumSteamOpt
{
    /// <summary>
    /// Cuts down how often SEUM talks to the Steam API. Nothing here adds or removes a feature: the
    /// patches either answer a question the game already asked with the answer it already got, or
    /// stop it re-asking on a timer that ignores whether anything could have changed.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony HarmonyInstance;

        private void Awake()
        {
            Log = Logger;
            SteamOptConfig.Bind(Config);

            if (!SteamOptConfig.Enabled.Value)
            {
                Log.LogInfo("SeumSteamOpt is disabled by config.");
                return;
            }

            Harmony harmony = HarmonyInstance = new Harmony(MyPluginInfo.PLUGIN_GUID);
            LeaderboardPatches.Apply(harmony);
            FriendsPatches.Apply(harmony);
            AchievementPatches.Apply(harmony);
            WorkshopPatches.Apply(harmony);

            // Cache invalidation needs Steam callbacks, which cannot be registered until the game's
            // SteamManager has run SteamAPI.Init. A DontDestroyOnLoad component waits for that and
            // survives the scene loads the menus do.
            SteamOptRuntime.Create();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
