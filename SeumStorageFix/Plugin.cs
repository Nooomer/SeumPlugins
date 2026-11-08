using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumStorageFix
{
    /// <summary>
    /// Fixes the "storage" family of bugs where state from one run leaks into the next run
    /// (see https://seum.online/docs, "Prohibited / Rules" 7-9):
    ///
    ///  - portal storage: restarting fast enough lets a queued level finish complete the fresh
    ///    run at ~0.000;
    ///  - teleport / death-barrier storage: activating a teleport (or roar) on the exact tick
    ///    of landing makes the landing refund the charge that the activation just spent, giving
    ///    free full-power teleports mid-air;
    ///  - power-up / object storage: a few per-run fields on the character survive the restart
    ///    because the game's own reset callback never clears them.
    ///
    /// On every level restart the plugin drops the pending state queue, invalidates the finish
    /// guard, and clears every leftover the original reset misses. A second patch keeps the
    /// landing refund from firing inside the activation window.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            StorageFixConfig.Bind(Config);

            if (!StorageFixConfig.Enabled.Value)
            {
                Log.LogInfo("SeumStorageFix is disabled by config.");
                return;
            }

            StoragePatches.Apply(new Harmony(MyPluginInfo.PLUGIN_GUID));
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
