using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SeumFreeCam
{
    /// <summary>
    /// A spectator camera for replays: detach from the runner's head and fly around the level while
    /// the recorded run plays out in front of you.
    ///
    /// It works because a SEUM replay is not a recorded picture — <c>Replay.simulateStep</c> only
    /// teleports the character along the recorded frames, and every projectile, platform and
    /// trigger around them is the live game reacting to recorded events. The camera is therefore
    /// free to be anywhere without changing what is being watched.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony HarmonyInstance;

        private void Awake()
        {
            Log = Logger;
            FreeCamConfig.Bind(Config);

            if (!FreeCamConfig.Enabled.Value)
            {
                Log.LogInfo("SeumFreeCam is disabled by config.");
                return;
            }

            HarmonyInstance = new Harmony(MyPluginInfo.PLUGIN_GUID);
            Patches.Apply(HarmonyInstance);
            FreeCamRuntime.Create();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
        }
    }
}
