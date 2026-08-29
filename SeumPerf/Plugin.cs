using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumPerf
{
    /// <summary>
    /// Performance patches for SEUM. Nothing here changes what the game does - the default set
    /// only stops it recomputing and reallocating things it already had. Anything that trades
    /// image quality for frames lives behind an opt-in config entry.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony HarmonyInstance;

        private void Awake()
        {
            Log = Logger;
            PerfConfig.Bind(Config);

            if (!PerfConfig.Enabled.Value)
            {
                Log.LogInfo("SeumPerf is disabled by config.");
                return;
            }

            Harmony harmony = HarmonyInstance = new Harmony(MyPluginInfo.PLUGIN_GUID);
            PatchSet.Apply(harmony);
            Patches.Apply(harmony);

            // The runtime half owns cache invalidation and the opt-in quality overrides. It is a
            // separate DontDestroyOnLoad object rather than this plugin component so the overlay
            // and the LateUpdate effect toggling keep working across scene loads.
            PerfRuntime.Create();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
