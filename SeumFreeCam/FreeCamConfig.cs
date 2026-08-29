using BepInEx.Configuration;
using UnityEngine;

namespace SeumFreeCam
{
    /// <summary>
    /// Everything the spectator camera reads from disk. The defaults deliberately avoid the keys
    /// the other plugins in this repository already took (F10 SeumPerf, F11 SeumInput, and
    /// Space / [ / ] / . / Home in SeumReplay), so all of them can be installed together.
    /// </summary>
    internal static class FreeCamConfig
    {
        internal static ConfigEntry<bool> Enabled;

        internal static ConfigEntry<KeyCode> ToggleKey;
        internal static ConfigEntry<KeyCode> UpKey;
        internal static ConfigEntry<KeyCode> DownKey;
        internal static ConfigEntry<KeyCode> FastKey;
        internal static ConfigEntry<KeyCode> SlowKey;
        internal static ConfigEntry<KeyCode> SnapKey;

        internal static ConfigEntry<float> MoveSpeed;
        internal static ConfigEntry<float> FastMultiplier;
        internal static ConfigEntry<float> SlowMultiplier;
        internal static ConfigEntry<float> LookSensitivity;
        internal static ConfigEntry<bool> InvertY;

        internal static ConfigEntry<bool> OnWinScreen;
        internal static ConfigEntry<bool> ShowMarker;
        internal static ConfigEntry<string> MarkerColor;
        internal static ConfigEntry<bool> HideHands;
        internal static ConfigEntry<bool> LockFov;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            OnWinScreen = cfg.Bind("01 - General", "OnWinScreen", true,
                "Also allow the free camera while your own run replays behind the win screen "
                + "(gameplay state FINISH_LEVEL), not just in the replay viewer opened from a "
                + "leaderboard.");

            ToggleKey = cfg.Bind("02 - Keys", "ToggleKey", KeyCode.F,
                "Enters and leaves the free camera. It starts exactly where the runner's view is, "
                + "so the switch is seamless.");
            UpKey = cfg.Bind("02 - Keys", "UpKey", KeyCode.E, "Rise while the free camera is on.");
            DownKey = cfg.Bind("02 - Keys", "DownKey", KeyCode.Q, "Descend while the free camera is on.");
            FastKey = cfg.Bind("02 - Keys", "FastKey", KeyCode.LeftShift, "Hold to fly faster.");
            SlowKey = cfg.Bind("02 - Keys", "SlowKey", KeyCode.LeftAlt, "Hold to fly slower, for close-ups.");
            SnapKey = cfg.Bind("02 - Keys", "SnapKey", KeyCode.C,
                "Teleport the camera back to the runner and look at them, without leaving free "
                + "camera mode. Useful after you have lost them behind a wall. "
                + "Do not rebind this, or any other key here, onto something the game itself still "
                + "listens for during a replay: R restarts the level and Escape opens the menu even "
                + "while a replay is playing, so those two would fire the game's action as well as "
                + "this plugin's.");

            MoveSpeed = cfg.Bind("03 - Movement", "MoveSpeed", 12f,
                "Base flying speed in units per second. The mouse wheel scales this live while the "
                + "free camera is on, and the value you land on is not written back to this file.");
            FastMultiplier = cfg.Bind("03 - Movement", "FastMultiplier", 5f, "Speed multiplier while FastKey is held.");
            SlowMultiplier = cfg.Bind("03 - Movement", "SlowMultiplier", 0.2f, "Speed multiplier while SlowKey is held.");

            LookSensitivity = cfg.Bind("03 - Movement", "LookSensitivity", 1f,
                "Multiplier on top of the game's own mouse sensitivity setting, which the free "
                + "camera reuses so that looking around feels the same as playing.");
            InvertY = cfg.Bind("03 - Movement", "InvertY", false,
                "Invert vertical look for the free camera only. The game's own Y modifier is "
                + "applied first, so leave this off if you already inverted look in the options.");

            ShowMarker = cfg.Bind("04 - View", "ShowMarker", true,
                "Draw a translucent capsule where the runner is. Without it the level looks empty "
                + "from the outside, because SEUM is first person and has no character model.");
            MarkerColor = cfg.Bind("04 - View", "MarkerColor", "FF4019A0",
                "Colour and opacity of that capsule, as RRGGBBAA hex. The alpha is what keeps the "
                + "runner visible when the camera ends up behind them.");
            HideHands = cfg.Bind("04 - View", "HideHands", true,
                "Stop the weapon camera from drawing the runner's hands, which would otherwise "
                + "float in the middle of the shot. The camera itself is left enabled so its "
                + "full-screen effects keep running.");
            LockFov = cfg.Bind("04 - View", "LockFov", true,
                "Hold the field of view at your configured value. The game widens it with the "
                + "runner's speed, which looks wrong once the camera is no longer on their head.");
        }
    }
}
