using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumTimerFix
{
    /// <summary>
    /// Fixes the SEUM "timer bug": after the game has been running for a long time (~70 minutes)
    /// the in-game timer gradually loses sync with the fixed 60-tick grid. The level time is
    /// computed as <c>Time.fixedTime - levelSpawnTime</c> in float32; once <c>Time.fixedTime</c>
    /// grows past a few thousand seconds, each fixed step lands on an ever coarser float grid
    /// (0.488 ms per ULP at ~70 minutes) and the accumulated rounding starts showing up as
    /// irregular timer steps and as runs that randomly gain or lose a few milliseconds.
    ///
    /// This plugin keeps its own double-precision tick counter and hands the game an exact
    /// <c>tickCount * fixedDeltaTime</c> value instead of the float subtraction, so the timer
    /// stays on the tick grid no matter how long the session has been running.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            TimerFixConfig.Bind(Config);

            if (!TimerFixConfig.Enabled.Value)
            {
                Log.LogInfo("SeumTimerFix is disabled by config.");
                return;
            }

            TimerPatches.Apply(new Harmony(MyPluginInfo.PLUGIN_GUID));
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
