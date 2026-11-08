using BepInEx.Configuration;
using UnityEngine;

namespace SeumTimerFix
{
    internal static class TimerFixConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> FixEntityPhases;
        internal static ConfigEntry<bool> Overlay;
        internal static ConfigEntry<KeyCode> OverlayKey;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            FixEntityPhases = cfg.Bind("03 - Entities", "FixEntityPhases", false,
                "Variant-B fix for the movement phases of entities that derive their phase from "
                + "absolute float32 session time: axes, rotators, chains, animated spikes and "
                + "moving platforms. Their vanilla formula (including the render-clock part) is "
                + "kept, only the accumulated float32 drift is replaced with an exact tick-grid "
                + "term, so they behave like on a freshly started game. Off by default because "
                + "the phases still contain the same tiny per-read float quantization the vanilla "
                + "game has.");

            Overlay = cfg.Bind("08 - Diagnostics", "Overlay", false,
                "Draw a small overlay with the timer drift: how far Unity's float Time.fixedTime "
                + "has moved off the exact tick grid (the timer bug), and how far the vanilla "
                + "level time would be off right now.");

            OverlayKey = cfg.Bind("08 - Diagnostics", "OverlayKey", KeyCode.F6,
                "Toggles the overlay at runtime.");
        }
    }
}
