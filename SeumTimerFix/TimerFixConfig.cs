using BepInEx.Configuration;

namespace SeumTimerFix
{
    internal static class TimerFixConfig
    {
        internal static ConfigEntry<bool> Enabled;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");
        }
    }
}
