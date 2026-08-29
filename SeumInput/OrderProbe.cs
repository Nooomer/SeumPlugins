using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// Answers one question that cannot be settled by reading the code: within a frame, does
    /// Rewired's InputManager refresh the device state before FPSInputController.Update reads the
    /// mouse axes, or after?
    ///
    /// Unity decides that by script execution order, which lives in project settings rather than in
    /// any assembly. If Rewired runs second, every look input the game acts on is a full frame old -
    /// which is worth far more milliseconds than anything else in this plugin.
    /// </summary>
    internal static class OrderProbe
    {
        private static int currentFrame = -1;
        private static bool rewiredSeenThisFrame;

        private static float rewiredFirstRatio = -1f;
        private static int samples;

        internal static bool Active { get; private set; }

        /// <summary>Share of frames where Rewired refreshed before the game read input, or -1.</summary>
        internal static float RewiredFirstRatio => rewiredFirstRatio;

        internal static int Samples => samples;

        internal static void Apply(Harmony harmony)
        {
            MethodBase rewiredUpdate = AccessTools.Method(typeof(Rewired.InputManager_Base), "Update");
            MethodBase gameUpdate = AccessTools.Method(typeof(FPSInputController), "Update");

            if (rewiredUpdate == null || gameUpdate == null)
            {
                Plugin.Log.LogWarning("SeumInput: update-order probe could not resolve its targets.");
                return;
            }

            try
            {
                harmony.Patch(rewiredUpdate,
                    new HarmonyMethod(AccessTools.Method(typeof(OrderProbe), nameof(MarkRewired))));
                harmony.Patch(gameUpdate,
                    new HarmonyMethod(AccessTools.Method(typeof(OrderProbe), nameof(MarkGame))));
                Active = true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"SeumInput: update-order probe failed: {ex.Message}");
            }
        }

        public static void MarkRewired()
        {
            NewFrameCheck();
            rewiredSeenThisFrame = true;
        }

        public static void MarkGame()
        {
            NewFrameCheck();

            float sample = rewiredSeenThisFrame ? 1f : 0f;
            rewiredFirstRatio = rewiredFirstRatio < 0f ? sample : Mathf.Lerp(rewiredFirstRatio, sample, 0.05f);
            samples++;
        }

        private static void NewFrameCheck()
        {
            int frame = Time.frameCount;
            if (frame != currentFrame)
            {
                currentFrame = frame;
                rewiredSeenThisFrame = false;
            }
        }
    }
}
