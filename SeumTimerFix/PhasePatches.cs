using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace SeumTimerFix
{
    /// <summary>
    /// Variant-B fix for the movement phases of entities that derive their phase from absolute
    /// float32 session time instead of stepping it locally:
    ///
    ///   AxeAnimation::simulate   sin((freezeTime  - Game.restartTimestamp + offset) * speed)
    ///   Rotator::FixedUpdate     angle += (Time.time - Game.restartTimestamp) * speed
    ///   ChainAnimation::Update   sin(Time.time * speed + randomOffset)
    ///   SpikesAnimated::FixedUpdate  Repeat(Time.time - stateChangeTime - restartTimestampOffset, period)
    ///   MovingPlatform::FixedUpdate   timeCounter = Time.time - Game.restartTimestamp
    ///
    /// Time.time / Time.fixedTime are float32 accumulators: after a long session every read is
    /// quantized to a coarse grid and the accumulated rounding drifts the phase by milliseconds.
    /// The drift cannot be subtracted from the stored floats (the rounding already destroyed the
    /// information), so instead each phase is recomputed from a drift-free equivalent of
    /// Time.time:
    ///
    ///   PhaseTime(t) = (Time.time - Time.fixedTime)                 // the vanilla render-clock
    ///                + FixedTick * (double)fixedDeltaTime           // offset, kept verbatim
    ///                                                                // (exact tick grid)
    ///
    /// The render-clock part is the exact same Sterbenz-exact float difference the vanilla
    /// formulas already see, so in a freshly started session the phases are identical; only the
    /// accumulated drift (and its random walk) is replaced by the exact tick term. The per-read
    /// quantization the vanilla game has is intentionally kept - the goal is "behave like a
    /// fresh session", not "be more deterministic than vanilla".
    ///
    /// Anchors: the restart anchor is captured in Game::restartLevel (where vanilla captures
    /// Game.restartTimestamp), and the spike cycle anchor in SpikesAnimated::setActive (the only
    /// writer of stateChangeTime; its restart callback calls it too).
    /// </summary>
    internal static class PhasePatches
    {
        private static readonly ConditionalWeakTable<SpikesAnimated, PhaseAnchor> SpikeAnchors =
            new ConditionalWeakTable<SpikesAnimated, PhaseAnchor>();

        private sealed class PhaseAnchor
        {
            internal double Value;
        }

        private static double PhaseAtRestart;

        private static FieldInfo FreezeTimeField;
        private static FieldInfo RestartTimestampField;
        private static MethodInfo GetTimeMethod;

        private static AccessTools.FieldRef<SpikesAnimated, float> StateChangeTimeRef;
        private static AccessTools.FieldRef<SpikesAnimated, float> StartDelayRef;

        private static MethodInfo GetCurrentPhaseTimeMethod;
        private static MethodInfo GetRestartAnchorMethod;
        private static MethodInfo GetSpikesTimeMethod;

        internal static void Apply(Harmony harmony)
        {
            if (!TimerFixConfig.FixEntityPhases.Value)
            {
                Plugin.Log.LogInfo("SeumTimerFix: entity phase fix is disabled by config.");
                return;
            }

            FreezeTimeField = AccessTools.Field(typeof(FPSInputController), "freezeTime");
            RestartTimestampField = AccessTools.Field(typeof(Game), "restartTimestamp");
            GetTimeMethod = AccessTools.PropertyGetter(typeof(Time), "time");
            StateChangeTimeRef = AccessTools.FieldRefAccess<SpikesAnimated, float>("stateChangeTime");
            StartDelayRef = AccessTools.FieldRefAccess<SpikesAnimated, float>("startDelay");

            if (FreezeTimeField == null || RestartTimestampField == null || GetTimeMethod == null
                || StateChangeTimeRef == null || StartDelayRef == null)
            {
                Plugin.Log.LogError("SeumTimerFix: entity phase fix could not resolve one of the "
                    + "fields/methods; the game build does not match the decompiled original. "
                    + "Entity phase patches are not applied.");
                return;
            }

            GetCurrentPhaseTimeMethod = AccessTools.Method(typeof(PhasePatches), nameof(GetCurrentPhaseTime));
            GetRestartAnchorMethod = AccessTools.Method(typeof(PhasePatches), nameof(GetRestartAnchor));
            GetSpikesTimeMethod = AccessTools.Method(typeof(PhasePatches), nameof(GetSpikesTime));

            // Before the first Game.restartLevel the vanilla anchor is -1 (random phase for one
            // frame); zero ours instead so the transient phase stays near the vanilla one.
            PhaseAtRestart = CurrentPhaseTime();

            harmony.PatchAll(typeof(PhasePatches));
            Plugin.Log.LogInfo("SeumTimerFix: entity phase fix armed (variant B: vanilla phase minus drift).");
        }

        internal static double CurrentPhaseTime()
        {
            return (double)(Time.time - Time.fixedTime)
                 + (double)TimerPatches.FixedTick * (double)Time.fixedDeltaTime;
        }

        internal static double GetCurrentPhaseTime()
        {
            return CurrentPhaseTime();
        }

        internal static double GetRestartAnchor()
        {
            return PhaseAtRestart;
        }

        internal static double GetSpikesTime(SpikesAnimated spikes)
        {
            return CurrentPhaseTime() - SpikeAnchors.GetOrCreateValue(spikes).Value
                 + (double)StateChangeTimeRef(spikes);
        }

        // ---------------------------------------------------------------------- anchors

        [HarmonyPatch(typeof(Game), "restartLevel")]
        private static class RestartAnchorPatch
        {
            private static void Postfix()
            {
                PhaseAtRestart = CurrentPhaseTime();
            }
        }

        [HarmonyPatch(typeof(SpikesAnimated), "setActive")]
        private static class SpikesAnchorPatch
        {
            private static void Postfix(SpikesAnimated __instance)
            {
                // Vanilla stores stateChangeTime = Time.time + startDelay, so the cycle anchor
                // carries the same delay.
                SpikeAnchors.GetOrCreateValue(__instance).Value =
                    CurrentPhaseTime() + (double)StartDelayRef(__instance);
            }
        }

        // ---------------------------------------------------------------------- phase rewrites

        [HarmonyPatch(typeof(AxeAnimation), "simulate")]
        private static class AxeSimulatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i + 2 < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldsfld && (code[i].operand as FieldInfo) == FreezeTimeField
                        && code[i + 1].opcode == OpCodes.Ldsfld && (code[i + 1].operand as FieldInfo) == RestartTimestampField
                        && code[i + 2].opcode == OpCodes.Sub)
                    {
                        // Push: PhaseTime - PhaseAtRestart + (freezeTime - Time.time) + restartTimestamp.
                        // The original instructions right after subtract restartTimestamp again, so
                        // the net result is PhaseTime - PhaseAtRestart + freezeTime - Time.time.
                        code[i] = new CodeInstruction(OpCodes.Call, GetCurrentPhaseTimeMethod);
                        code.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Call, GetRestartAnchorMethod),
                            new CodeInstruction(OpCodes.Sub),
                            new CodeInstruction(OpCodes.Ldsfld, FreezeTimeField),
                            new CodeInstruction(OpCodes.Call, GetTimeMethod),
                            new CodeInstruction(OpCodes.Sub),
                            new CodeInstruction(OpCodes.Add),
                            new CodeInstruction(OpCodes.Ldsfld, RestartTimestampField),
                            new CodeInstruction(OpCodes.Add),
                            new CodeInstruction(OpCodes.Conv_R4),
                        });
                        return code;
                    }
                }

                Plugin.Log.LogError("SeumTimerFix: AxeAnimation.simulate pattern not found; phase not rewritten.");
                return code;
            }
        }

        [HarmonyPatch(typeof(Rotator), "FixedUpdate")]
        private static class RotatorFixedUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Call && (code[i].operand as MethodInfo) == GetTimeMethod)
                    {
                        // Push PhaseTime - PhaseAtRestart + restartTimestamp; the original then
                        // subtracts restartTimestamp, leaving PhaseTime - PhaseAtRestart.
                        code[i] = new CodeInstruction(OpCodes.Call, GetCurrentPhaseTimeMethod);
                        code.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Call, GetRestartAnchorMethod),
                            new CodeInstruction(OpCodes.Sub),
                            new CodeInstruction(OpCodes.Ldsfld, RestartTimestampField),
                            new CodeInstruction(OpCodes.Add),
                            new CodeInstruction(OpCodes.Conv_R4),
                        });
                        return code;
                    }
                }

                Plugin.Log.LogError("SeumTimerFix: Rotator.FixedUpdate Time.time not found; phase not rewritten.");
                return code;
            }
        }

        [HarmonyPatch(typeof(ChainAnimation), "Update")]
        private static class ChainAnimationUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Call && (code[i].operand as MethodInfo) == GetTimeMethod)
                    {
                        code[i] = new CodeInstruction(OpCodes.Call, GetCurrentPhaseTimeMethod);
                        code.Insert(i + 1, new CodeInstruction(OpCodes.Conv_R4));
                        return code;
                    }
                }

                Plugin.Log.LogError("SeumTimerFix: ChainAnimation.Update Time.time not found; phase not rewritten.");
                return code;
            }
        }

        [HarmonyPatch(typeof(SpikesAnimated), "FixedUpdate")]
        private static class SpikesFixedUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Call && (code[i].operand as MethodInfo) == GetTimeMethod)
                    {
                        // Push PhaseTime - spikeAnchor + stateChangeTime; the original then
                        // subtracts stateChangeTime and restartTimestampOffset, leaving
                        // PhaseTime - spikeAnchor - restartTimestampOffset.
                        code[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        code.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Call, GetSpikesTimeMethod),
                            new CodeInstruction(OpCodes.Conv_R4),
                        });
                        return code;
                    }
                }

                Plugin.Log.LogError("SeumTimerFix: SpikesAnimated.FixedUpdate Time.time not found; phase not rewritten.");
                return code;
            }
        }

        [HarmonyPatch(typeof(MovingPlatform), "FixedUpdate")]
        private static class MovingPlatformFixedUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Call && (code[i].operand as MethodInfo) == GetTimeMethod)
                    {
                        // Same rewrite as Rotator: timeCounter = PhaseTime - PhaseAtRestart.
                        code[i] = new CodeInstruction(OpCodes.Call, GetCurrentPhaseTimeMethod);
                        code.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Call, GetRestartAnchorMethod),
                            new CodeInstruction(OpCodes.Sub),
                            new CodeInstruction(OpCodes.Ldsfld, RestartTimestampField),
                            new CodeInstruction(OpCodes.Add),
                            new CodeInstruction(OpCodes.Conv_R4),
                        });
                        return code;
                    }
                }

                Plugin.Log.LogError("SeumTimerFix: MovingPlatform.FixedUpdate Time.time not found; phase not rewritten.");
                return code;
            }
        }
    }
}
