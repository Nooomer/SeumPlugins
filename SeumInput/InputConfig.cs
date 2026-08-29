using BepInEx.Configuration;
using UnityEngine;

namespace SeumInput
{
    /// <summary>Window mode override, with an explicit "leave it alone" value.</summary>
    internal enum WindowMode
    {
        Unchanged = 0,
        ExclusiveFullScreen,
        FullScreenWindow,
        MaximizedWindow,
        Windowed,
    }

    internal static class InputConfig
    {
        internal const int Unchanged = -1;

        internal static ConfigEntry<bool> Enabled;

        // --- IMGUI event flood -------------------------------------------------------------
        internal static ConfigEntry<bool> SkipMouseMoveEvents;

        // --- latency ------------------------------------------------------------------------
        internal static ConfigEntry<int> MaxQueuedFrames;
        internal static ConfigEntry<WindowMode> FullScreenMode;

        // --- diagnostics ---------------------------------------------------------------------
        internal static ConfigEntry<bool> Overlay;
        internal static ConfigEntry<KeyCode> OverlayKey;
        internal static ConfigEntry<bool> ProbeUpdateOrder;
        internal static ConfigEntry<bool> ProbeFramePhases;
        internal static ConfigEntry<float> StallThresholdMs;
        internal static ConfigEntry<bool> LogRewiredReport;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false the plugin does nothing at all.");

            SkipMouseMoveEvents = cfg.Bind("02 - IMGUI", "SkipMouseMoveEvents", false,
                "OnGUI is invoked once per input event, not once per frame, so a high polling rate "
                + "mouse multiplies the cost of the HUD by the number of mouse events in the frame. "
                + "The game never reads EventType.MouseMove (its IMGUI code only looks at Repaint, "
                + "MouseDown, MouseUp and MouseDrag, and hover is computed from GameCursor.position, "
                + "which Hud.Update fills from Input.mousePosition), so those events can be dropped "
                + "before the drawing code runs. Dragging and clicking are unaffected: holding a "
                + "button produces MouseDrag, not MouseMove. "
                + "Measured off by default: on the setup this was written against the histogram "
                + "shows a flat 2.0 OnGUI calls per frame even with an 8 kHz mouse moving fast, so "
                + "there is nothing here to drop. Kept as a switch because the event histogram is "
                + "the thing worth watching, and other setups may differ.");

            MaxQueuedFrames = cfg.Bind("03 - Latency", "MaxQueuedFrames", 1,
                new ConfigDescription(
                    "QualitySettings.maxQueuedFrames - how many frames the CPU may run ahead of the "
                    + "GPU. The game never sets it, so it runs at the engine default of 2. Lowering "
                    + "it to 1 removes a frame of render-queue latency; it costs throughput only "
                    + "when GPU bound, which this game is not. -1 leaves it alone.",
                    new AcceptableValueRange<int>(Unchanged, 4)));

            FullScreenMode = cfg.Bind("03 - Latency", "FullScreenMode", WindowMode.Unchanged,
                "Exclusive fullscreen generally presents with less latency than a borderless "
                + "window. Applied once at startup and whenever this setting changes, not "
                + "continuously, so the in-game options screen stays in charge afterwards.");

            Overlay = cfg.Bind("04 - Diagnostics", "Overlay", false,
                "Show the per-frame IMGUI event histogram: how many times OnGUI is called each "
                + "frame and which event types those calls are. This is the direct measurement of "
                + "the high-polling-rate problem.");

            OverlayKey = cfg.Bind("04 - Diagnostics", "OverlayKey", KeyCode.F11,
                "Toggles the overlay at runtime.");

            ProbeUpdateOrder = cfg.Bind("04 - Diagnostics", "ProbeUpdateOrder", false,
                "Record whether Rewired's InputManager updates before or after the game reads the "
                + "mouse in FPSInputController.Update. If it runs after, every look input is a full "
                + "frame stale. Shown in the overlay.");

            ProbeFramePhases = cfg.Bind("04 - Diagnostics", "ProbeFramePhases", true,
                "Split the frame into scripts / rendering / present+pump and show the average and "
                + "the worst of each. This is how you locate a stall that the method profiler has "
                + "already proven is not in any script: mouse messages are drained by the message "
                + "pump, so spikes landing in present+pump mean the cost is in the engine's input "
                + "handling or in the present call, and no patch can reach it.");

            StallThresholdMs = cfg.Bind("04 - Diagnostics", "StallThresholdMs", 16f,
                "A frame longer than this counts as a stall in the overlay's stalls-per-second "
                + "line. Use that rate, not the single worst frame, when comparing two settings - "
                + "one bad frame is noise, a rate is a measurement.");

            LogRewiredReport = cfg.Bind("04 - Diagnostics", "LogRewiredReport", true,
                "Dump Rewired's runtime configuration to the BepInEx log at startup.");

            VerboseLogging = cfg.Bind("04 - Diagnostics", "VerboseLogging", false,
                "Log every individual patch as it is applied.");
        }
    }
}
