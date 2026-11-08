using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SeumStorageFix
{
    /// <summary>
    /// Two halves.
    ///
    /// <b>Restart deep-clean</b> (Game.restartLevel postfix): the game's own RestartLevelCallback
    /// chain already resets almost everything, but it misses the queued state transitions
    /// (a FINISH_LEVEL queued the instant the player touched the portal survives the restart and
    /// completes the fresh run at ~0.000 — the "portal storage"), the finish guard, and a handful
    /// of per-run fields on the character. This patch drops the queue, re-arms the guard and
    /// clears those leftovers.
    ///
    /// <b>Recharge race</b> (FPSInputController.performLand): performLand refunds
    /// teleportsUsedInAir / roarChargeCount on every landing. Activating a teleport (or using a
    /// roar) on the exact tick of landing spends the charge and immediately gets it refunded by
    /// the same landing, so the player keeps full charges mid-air and can fire free full-power
    /// teleports. The patch remembers the pre-landing counters and restores them when a landing
    /// happens inside 0.2 s of an activation — the same window the game itself uses as the
    /// teleport cooldown. Landings outside that window recharge exactly as in vanilla.
    ///
    /// Everything the plugin actually prevents can be logged (LogPreventedStorages, on by
    /// default): dropped pending state transitions, cleared leftovers and suppressed refunds.
    /// </summary>
    internal static class StoragePatches
    {
        internal const float RechargeRaceWindow = 0.2f;

        private static FieldInfo NextStateStackField;
        private static FieldInfo LastFinishSessionIdField;
        private static FieldInfo GameSessionIdField;
        private static FieldInfo LevelFinishTimeOffsetField;
        private static FieldInfo WaitForNextCycleTimestampField;
        private static FieldInfo RoarTimestampField;
        private static FieldInfo TeleportedInAirTimeField;

        internal static void Apply(Harmony harmony)
        {
            NextStateStackField = AccessTools.Field(typeof(GameManager), "nextStateStack");
            LastFinishSessionIdField = AccessTools.Field(typeof(GameManager), "lastFinishSessionId");
            GameSessionIdField = AccessTools.Field(typeof(GameManager), "gameSessionId");
            LevelFinishTimeOffsetField = AccessTools.Field(typeof(GameManager), "levelFinishTimeOffset");
            WaitForNextCycleTimestampField = AccessTools.Field(typeof(FPSInputController), "waitForNextCycleTimestamp");
            RoarTimestampField = AccessTools.Field(typeof(FPSInputController), "roarTimestamp");
            TeleportedInAirTimeField = AccessTools.Field(typeof(FPSInputController), "teleportedInAirTime");

            if (NextStateStackField == null || LastFinishSessionIdField == null
                || GameSessionIdField == null || LevelFinishTimeOffsetField == null
                || WaitForNextCycleTimestampField == null || RoarTimestampField == null
                || TeleportedInAirTimeField == null)
            {
                Plugin.Log.LogError("SeumStorageFix: could not resolve one of the private fields; "
                    + "the game build does not match the decompiled original. No patch applied.");
                return;
            }

            harmony.PatchAll(typeof(StoragePatches));
            Plugin.Log.LogInfo("SeumStorageFix: restart deep-clean and recharge-race guard armed.");
        }

        // ------------------------------------------------------------------ restart deep-clean

        [HarmonyPatch(typeof(Game), "restartLevel")]
        private static class RestartLevelPatch
        {
            private static void Postfix()
            {
                CleanupPendingRunState();
            }
        }

        private static void CleanupPendingRunState()
        {
            List<string> prevented = new List<string>();

            GameManager manager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (manager != null)
            {
                // A FINISH_LEVEL / FAIL_LEVEL queued by the old run must not fire in the fresh
                // one — that is the 0.000 "portal storage". Drop the whole pending queue.
                var queue = (Queue<GameManager.GameplayState>)NextStateStackField.GetValue(manager);
                if (queue != null)
                {
                    if (queue.Count > 0)
                    {
                        var states = new GameManager.GameplayState[queue.Count];
                        queue.CopyTo(states, 0);
                        prevented.Add("dropped " + states.Length + " pending state transition(s) on restart: "
                            + string.Join(", ", states) + " (portal storage)");
                        queue.Clear();
                    }
                }

                // finishLevel() only runs once per session: it compares gameSessionId against
                // lastFinishSessionId. Force them equal so any late finish of the old run is
                // rejected after the restart.
                int sessionId = (int)GameSessionIdField.GetValue(manager);
                int lastFinish = (int)LastFinishSessionIdField.GetValue(manager);
                if (lastFinish != sessionId)
                {
                    prevented.Add("finish guard re-armed: lastFinishSessionId " + lastFinish + " -> " + sessionId);
                }

                LastFinishSessionIdField.SetValue(manager, sessionId);

                // Discard half-finished timing state with the run. The IN_GAME transition of the
                // next run re-arms the capture itself.
                float offset = (float)LevelFinishTimeOffsetField.GetValue(manager);
                if (offset != 0f)
                {
                    prevented.Add("discarded levelFinishTimeOffset " + offset.ToString("0.0000"));
                }

                LevelFinishTimeOffsetField.SetValue(manager, 0f);

                if (manager.captureLevelSpawnTime)
                {
                    prevented.Add("discarded stale spawn-time capture");
                }

                manager.captureLevelSpawnTime = false;
                GameManager.firstInputReceived = false;
            }

            FPSInputController controller = UnityEngine.Object.FindObjectOfType<FPSInputController>();
            if (controller != null)
            {
                // The projectiles themselves die via their own restart callbacks; the character's
                // references to them are what the original reset never clears.
                if (controller.activeTeleportProjectile != null)
                {
                    if (!controller.activeTeleportProjectile.destroyed)
                    {
                        controller.activeTeleportProjectile.kill(null, null);
                    }

                    prevented.Add("cleared leftover teleport projectile");
                }

                controller.activeTeleportProjectile = null;

                if (controller.activePlatformProjectile != null)
                {
                    if (!controller.activePlatformProjectile.destroyed)
                    {
                        controller.activePlatformProjectile.kill(null, null);
                    }

                    prevented.Add("cleared leftover platform projectile");
                }

                controller.activePlatformProjectile = null;

                if (controller.teleportSignalObject != null
                    || controller.teleportExitSignal || controller.teleportKillSignal)
                {
                    prevented.Add("cleared leftover teleport signals");
                }

                controller.teleportSignalObject = null;
                controller.teleportExitSignal = false;
                controller.teleportKillSignal = false;
                controller.teleportSignalNormal = Vector3.zero;

                if (controller.portalPassTime >= 0f)
                {
                    prevented.Add("cleared portal-pass timer " + controller.portalPassTime.ToString("0.000"));
                }

                controller.portalPassTime = -1f;

                float waitForNextCycle = (float)WaitForNextCycleTimestampField.GetValue(controller);
                if (waitForNextCycle >= 0f)
                {
                    prevented.Add("cleared ground-teleport cycle gate " + waitForNextCycle.ToString("0.000"));
                }

                WaitForNextCycleTimestampField.SetValue(controller, -1f);

                if (controller.handEqupied)
                {
                    prevented.Add("cleared equipped hand carried over from the previous run");
                }

                controller.handEqupied = false;
                controller.handEqupiedTimestamp = -10f;

                if (controller.followPath != null)
                {
                    prevented.Add("cleared leftover follow-path");
                }

                controller.followPath = null;
                controller.followPathTime = 0f;

                float roarTimestamp = (float)RoarTimestampField.GetValue(controller);
                if (roarTimestamp >= 0f || controller.roarChargeCount != 0)
                {
                    prevented.Add("cleared roar state (charges " + controller.roarChargeCount
                        + ", timestamp " + roarTimestamp.ToString("0.000") + ")");
                }

                RoarTimestampField.SetValue(controller, -1f);
                controller.roarChargeCount = 0;

                float teleportedInAirTime = (float)TeleportedInAirTimeField.GetValue(controller);
                if (teleportedInAirTime >= 0f)
                {
                    prevented.Add("cleared teleport activation state (teleportedInAirTime "
                        + teleportedInAirTime.ToString("0.000") + ")");
                }

                TeleportedInAirTimeField.SetValue(controller, -1f);

                CharacterMotor motor = controller.GetComponent<CharacterMotor>();
                if (motor != null)
                {
                    if (motor.teleportsUsedInAir != 0)
                    {
                        prevented.Add("reset teleportsUsedInAir " + motor.teleportsUsedInAir + " -> 0");
                    }

                    motor.teleportsUsedInAir = 0;
                }

                if (controller.characterView != null)
                {
                    if (controller.characterView.chargesActive != 3)
                    {
                        prevented.Add("reset chargesActive " + controller.characterView.chargesActive + " -> 3");
                    }

                    controller.characterView.chargesActive = 3;
                }
            }

            // Finish / death UI leftovers must not leak into the fresh run.
            if (Hud.winUIFinishedFadingIn)
            {
                prevented.Add("cleared finish UI state (winUIFinishedFadingIn)");
            }

            Hud.winUIFinishedFadingIn = false;

            if (Hud.deathTimer > 0f || Hud.portalReachedTimer > 0f)
            {
                prevented.Add("cleared death/portal HUD timers (death " + Hud.deathTimer.ToString("0.000")
                    + ", portal " + Hud.portalReachedTimer.ToString("0.000") + ")");
            }

            Hud.deathTimer = 0f;
            Hud.portalReachedTimer = 0f;

            if (prevented.Count > 0 && StorageFixConfig.LogPreventedStorages.Value)
            {
                Plugin.Log.LogInfo("SeumStorageFix: restart cleaned up " + prevented.Count
                    + " leftover(s) from the previous run:");
                for (int i = 0; i < prevented.Count; i++)
                {
                    Plugin.Log.LogInfo("  " + prevented[i]);
                }
            }
        }

        // ---------------------------------------------------------------------- recharge race

        [HarmonyPatch(typeof(FPSInputController), "performLand")]
        private static class PerformLandPatch
        {
            private static int teleportsUsedInAir;
            private static int chargesActive;
            private static int roarChargeCount;

            private static void Prefix(FPSInputController __instance)
            {
                CharacterMotor motor = __instance.GetComponent<CharacterMotor>();
                teleportsUsedInAir = motor != null ? motor.teleportsUsedInAir : 0;
                chargesActive = __instance.characterView != null
                    ? __instance.characterView.chargesActive
                    : 0;
                roarChargeCount = __instance.roarChargeCount;
            }

            private static void Postfix(FPSInputController __instance)
            {
                if (!StorageFixConfig.FixRechargeRace.Value)
                {
                    return;
                }

                float now = Time.time;
                float teleportedInAirTime = (float)TeleportedInAirTimeField.GetValue(__instance);
                float roarTimestamp = (float)RoarTimestampField.GetValue(__instance);

                bool teleportInWindow = teleportedInAirTime >= 0f
                    && now - teleportedInAirTime < RechargeRaceWindow;
                bool roarInWindow = roarTimestamp >= 0f
                    && now - roarTimestamp < RechargeRaceWindow;

                if (!teleportInWindow && !roarInWindow)
                {
                    return;
                }

                // The landing inside the activation window must not refund what the activation
                // spent. Restore the pre-landing counters; the next landing (outside the window)
                // recharges normally.
                if (teleportInWindow)
                {
                    CharacterMotor motor = __instance.GetComponent<CharacterMotor>();
                    if (motor != null)
                    {
                        motor.teleportsUsedInAir = teleportsUsedInAir;
                    }

                    if (__instance.characterView != null)
                    {
                        __instance.characterView.chargesActive = chargesActive;
                    }

                    if (StorageFixConfig.LogPreventedStorages.Value)
                    {
                        Plugin.Log.LogInfo("SeumStorageFix: landing " + (now - teleportedInAirTime).ToString("0.000")
                            + " s after teleport activation - charge refund suppressed (restored teleportsUsedInAir "
                            + teleportsUsedInAir + ", chargesActive " + chargesActive + ")");
                    }
                }

                if (roarInWindow)
                {
                    __instance.roarChargeCount = roarChargeCount;

                    if (StorageFixConfig.LogPreventedStorages.Value)
                    {
                        Plugin.Log.LogInfo("SeumStorageFix: landing " + (now - roarTimestamp).ToString("0.000")
                            + " s after roar use - charge refund suppressed (restored roarChargeCount "
                            + roarChargeCount + ")");
                    }
                }
            }
        }
    }
}
