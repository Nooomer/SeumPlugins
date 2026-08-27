using BepInEx;
using BepInEx.Logging;

namespace VelocityMeter
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            // Harmony patches only, no ModLoader.Initialize() here: BepInEx plugins Awake()
            // before the game's own bootstrap runs, so GameSettings.settings (and other
            // game-static state ModLoader's constructor reads) is still null at this point.
            // ModLoader is created later, from the IntroSplashStartPatch below - the same
            // point the original mod itself used to initialize.
            ModLoaderPatches.Apply();
        }

        private void OnGUI()
        {
            ReplayInputOverlay.Draw();
        }
    }
}
