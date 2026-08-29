using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumReplay
{
    /// <summary>
    /// Replay loading for SEUM: a correct decoder, a disk cache, a download path that cannot get
    /// stuck, and a screen that says what it is waiting for.
    ///
    /// The recording side of <c>Replay</c> is untouched on purpose. Those bytes go up to the Steam
    /// leaderboard and every unmodded client has to be able to read them back, so this plugin only
    /// ever changes how replays are read, fetched and played.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony HarmonyInstance;

        private void Awake()
        {
            Log = Logger;
            ReplayConfig.Bind(Config);

            if (!ReplayConfig.Enabled.Value)
            {
                Log.LogInfo("SeumReplay is disabled by config.");
                return;
            }

            GameRefs.Resolve();
            ReplayCache.Init();

            HarmonyInstance = new Harmony(MyPluginInfo.PLUGIN_GUID);
            Patches.Apply(HarmonyInstance);

            ReplayRuntime.Create();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
