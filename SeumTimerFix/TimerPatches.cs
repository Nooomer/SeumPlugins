using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SeumTimerFix
{
    /// <summary>
    /// The original timing chain (all float32):
    ///
    ///   GameManager.calculateCurrentLevelTime() = Time.fixedTime - levelSpawnTime + levelFinishTimeOffset
    ///   levelSpawnTime = Time.fixedTime, captured on the first input of the run
    ///   finish score    = (int)(time * 1000)
    ///
    /// Time.fixedTime only has ~0.5 ms of float32 precision once the session is an hour old, and
    /// it accumulates one rounded float addition per physics step, so the level time drifts off
    /// the 60 Hz tick grid and the final score randomly gains or loses a few milliseconds.
    ///
    /// The replacement counts physics steps in a long and multiplies by (double)fixedDeltaTime,
    /// which is exact on the tick grid for any session length. The spawn anchor is captured at
    /// the same two places the original captures levelSpawnTime:
    ///   1. GameManager.FixedUpdate, the frame it consumes captureLevelSpawnTime;
    ///   2. GameManager.gameplayStateChanged, when it leaves IN_GAME (play-time accounting).
    /// Between the two the values agree with the original to sub-microsecond drift.
    /// </summary>
    internal static class TimerPatches
    {
        /// <summary>Physics steps since plugin load — the drift-free clock.</summary>
        internal static long FixedTick;

        /// <summary>The tick the current run's spawn time was captured on.</summary>
        internal static long SpawnTick;

        private static FieldInfo LevelFinishTimeOffsetField;
        private static FieldInfo PreviousStateField;

        internal static void Apply(Harmony harmony)
        {
            LevelFinishTimeOffsetField = AccessTools.Field(typeof(GameManager), "levelFinishTimeOffset");
            PreviousStateField = AccessTools.Field(typeof(GameManager), "previousState");

            if (LevelFinishTimeOffsetField == null || PreviousStateField == null)
            {
                Plugin.Log.LogError("SeumTimerFix: could not resolve GameManager private fields; "
                    + "the game build does not match the decompiled original. No patch applied.");
                return;
            }

            harmony.PatchAll(typeof(TimerPatches));
            Plugin.Log.LogInfo("SeumTimerFix: tick clock armed; level time is now tick-exact.");

            // Optional variant-B rewrite of the entity movement phases (axes, rotators, chains,
            // animated spikes, moving platforms); off unless FixEntityPhases is enabled.
            PhasePatches.Apply(harmony);
        }

        [HarmonyPatch(typeof(GameManager), "FixedUpdate")]
        private static class GameManagerFixedUpdatePatch
        {
            // Whether captureLevelSpawnTime was true when this physics step started; the original
            // consumes it (true -> false) inside the same call when it captures levelSpawnTime.
            private static bool wasCapturing;

            private static void Prefix(GameManager __instance)
            {
                FixedTick++;
                wasCapturing = __instance.captureLevelSpawnTime;
            }

            private static void Postfix(GameManager __instance)
            {
                if (wasCapturing && !__instance.captureLevelSpawnTime)
                {
                    SpawnTick = FixedTick;
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "gameplayStateChanged")]
        private static class GameManagerGameplayStateChangedPatch
        {
            // The original captures levelSpawnTime = Time.fixedTime here when the run ends
            // (previousState == IN_GAME -> START_LEVEL_AIM / FINISH_LEVEL / FAIL_LEVEL), right
            // after adding the run time to the play-time total. Re-anchor our clock to the same
            // instant so every later calculateCurrentLevelTime() call sees the same zero point.
            private static void Postfix(GameManager __instance)
            {
                GameManager.GameplayState previous = (GameManager.GameplayState)PreviousStateField.GetValue(__instance);
                if (previous != GameManager.GameplayState.IN_GAME)
                {
                    return;
                }

                if (__instance.gameplayState == GameManager.GameplayState.START_LEVEL_AIM
                    || __instance.gameplayState == GameManager.GameplayState.FINISH_LEVEL
                    || __instance.gameplayState == GameManager.GameplayState.FAIL_LEVEL)
                {
                    SpawnTick = FixedTick;
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "calculateCurrentLevelTime")]
        private static class CalculateCurrentLevelTimePatch
        {
            private static bool Prefix(GameManager __instance, ref float __result)
            {
                long ticks = FixedTick - SpawnTick;
                if (ticks < 0)
                {
                    ticks = 0;
                }

                float offset = (float)LevelFinishTimeOffsetField.GetValue(__instance);
                double time = (double)ticks * (double)Time.fixedDeltaTime + (double)offset;
                __result = (float)time;
                return false;
            }
        }
    }
}
