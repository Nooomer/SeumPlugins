using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumLimit
{
    /// <summary>
    /// Estimates the time limit of a level from its top leaderboard replays: a sum of best
    /// segments across the top runs (a pace every piece of which somebody actually played), and a
    /// per-tick loss estimate on the world record's own path (where it still leaks time).
    ///
    /// Both are shown on the aim screen (<see cref="TopRuns"/>), and after a finish the run just
    /// played is reviewed against them (<see cref="RunReview"/>). The F7 dump and the live-run
    /// recorder are the diagnostics the models were fitted with; the recorder is off by default.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            LimitConfig.Bind(Config);

            if (!LimitConfig.Enabled.Value)
            {
                Log.LogInfo("SeumLimit is disabled by config.");
                return;
            }

            LiveRecorder.Apply(new Harmony(MyPluginInfo.PLUGIN_GUID));
            Housekeeping.Run();
            LimitRuntime.Create();
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
