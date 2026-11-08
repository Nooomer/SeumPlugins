using BepInEx.Configuration;
using UnityEngine;

namespace SeumTimerFix
{
    internal static class TimerFixConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> Overlay;
        internal static ConfigEntry<KeyCode> OverlayKey;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            Overlay = cfg.Bind("08 - Diagnostics", "Overlay", false,
                "Draw a small overlay with the timer drift: how far Unity's float Time.fixedTime "
                + "has moved off the exact tick grid (the timer bug), and how far the vanilla "
                + "level time would be off right now.");

            OverlayKey = cfg.Bind("08 - Diagnostics", "OverlayKey", KeyCode.F6,
                "Toggles the overlay at runtime.");
        }
    }
}
