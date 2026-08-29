using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumInput
{
    /// <summary>
    /// Input latency and high-polling-rate fixes for SEUM.
    ///
    /// The headline problem: OnGUI runs once per input event, not once per frame, and the game's
    /// leaderboard drawing does real work outside its Repaint guard. An 8 kHz mouse therefore turns
    /// one HUD pass per frame into dozens. This plugin drops the events the game cannot use, and
    /// gives you the histogram to prove it.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony HarmonyInstance;

        private void Awake()
        {
            Log = Logger;
            InputConfig.Bind(Config);

            if (!InputConfig.Enabled.Value)
            {
                Log.LogInfo("SeumInput is disabled by config.");
                return;
            }

            HarmonyInstance = new Harmony(MyPluginInfo.PLUGIN_GUID);
            GuiEvents.Apply(HarmonyInstance);
            InputRuntime.Create();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
