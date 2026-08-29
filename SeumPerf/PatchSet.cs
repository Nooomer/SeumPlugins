using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SeumPerf
{
    /// <summary>
    /// Applies the generic IL rewrites to a hand-picked list of methods.
    ///
    /// The list is deliberately explicit rather than "every method in the assembly": component
    /// caching is only unconditionally safe in code that runs every frame against a stable
    /// hierarchy, and a blanket rewrite would also hit level loading and the editor, where objects
    /// are created and destroyed constantly.
    /// </summary>
    internal static class PatchSet
    {
        /// <summary>Per-frame methods that repeat GetComponent-style lookups.</summary>
        private static readonly string[] ComponentLookupTargets =
        {
            "GameManager:FixedUpdate",
            "GameManager:Update",
            "GameManager:handleCollisions",
            "FPSInputController:Update",
            "CharacterMotor:performFixedUpdate",
            "HandEffects:Update",
            "HandLightSpin:Update",
            "Mine:Update",
            "MaterialRenderQueue:Update",
            "Projectile:Update",
            "Dart:FixedUpdate",
            "Game:LateUpdate",
            "GhostPlatform:Update",
            "DelayedDestroy:Update",
            "RingTrigger:syncRotation",
            "PortalRenderer:OnWillRenderObject",
            "LevelSelector:Update",
        };

        /// <summary>The two per-frame callers of Camera.main.</summary>
        private static readonly string[] CameraMainTargets =
        {
            "Projectile:Update",
            "Dart:FixedUpdate",
        };

        /// <summary>Per-frame code that addresses shader properties by string.</summary>
        private static readonly string[] ShaderPropertyTargets =
        {
            "HandEffects:Update",
            "HandLightSpin:Update",
            "JumpPlateBlink:Update",
            "Mine:Update",
            "DelayedDestroy:Update",
            "GhostOverlayEffect:OnRenderImage",
            "ShadowWorldOverlayEffect:OnRenderImage",
            "ExposureCorrection:OnRenderImage",
            "SSAOPro:OnRenderImage",
            "PortalRenderer:OnWillRenderObject",
        };

        internal static void Apply(Harmony harmony)
        {
            if (PerfConfig.CacheComponentLookups.Value)
            {
                ApplyTranspiler(harmony, ComponentLookupTargets,
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.CacheComponentLookups)));
            }

            if (PerfConfig.CacheCameraMain.Value)
            {
                ApplyTranspiler(harmony, CameraMainTargets,
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.CacheCameraMain)));
            }

            if (PerfConfig.CacheShaderPropertyIds.Value)
            {
                ApplyTranspiler(harmony, ShaderPropertyTargets,
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.CacheShaderPropertyIds)));
            }

            if (PerfConfig.CachePortalMaterials.Value)
            {
                ApplyTranspiler(harmony, new[] { "PortalRenderer:OnWillRenderObject" },
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.CacheRendererMaterials)));
            }

            if (PerfConfig.GuardInputString.Value)
            {
                ApplyTranspiler(harmony, new[] { "Game:commonLateUpdate" },
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.GuardInputString)));
            }

            if (PerfConfig.StripCollisionDebugStrings.Value)
            {
                ApplyTranspiler(harmony, new[] { "CollisionRule:reportCollision" },
                    AccessTools.Method(typeof(ConcatStrippers), nameof(ConcatStrippers.KeepSecond)));
                ApplyTranspiler(harmony, new[] { "Projectile:castCollisions" },
                    AccessTools.Method(typeof(ConcatStrippers), nameof(ConcatStrippers.KeepFirst)));
            }

            if (PerfConfig.TrimProjectilePaths.Value)
            {
                ApplyTranspiler(harmony, new[] { "Path:.ctor" },
                    AccessTools.Method(typeof(PathBuffer), nameof(PathBuffer.Resize)));
            }

            if (PerfConfig.SkipUnusedAimPath.Value)
            {
                ApplyTranspiler(harmony, new[] { "FPSInputController:Update" },
                    AccessTools.Method(typeof(Rewrites), nameof(Rewrites.GuardAimPath)));
            }
        }

        private static void ApplyTranspiler(Harmony harmony, IEnumerable<string> targets, MethodInfo transpiler)
        {
            foreach (string target in targets)
            {
                MethodBase method = TypeResolver.ResolveMethod(target);
                if (method == null)
                {
                    Plugin.Log.LogWarning($"SeumPerf: could not resolve '{target}', skipping.");
                    continue;
                }

                try
                {
                    harmony.Patch(method, transpiler: new HarmonyMethod(transpiler));
                    if (PerfConfig.VerboseLogging.Value)
                    {
                        Plugin.Log.LogInfo($"SeumPerf: {transpiler.Name} -> {target}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"SeumPerf: {transpiler.Name} failed on '{target}': {ex.Message}");
                }
            }
        }

    }

    /// <summary>Thin wrappers so the parameterised rewrites have Harmony-compatible signatures.</summary>
    internal static class ConcatStrippers
    {
        internal static IEnumerable<CodeInstruction> KeepFirst(IEnumerable<CodeInstruction> instructions)
        {
            return Rewrites.DropConcat(instructions, keepFirst: true);
        }

        internal static IEnumerable<CodeInstruction> KeepSecond(IEnumerable<CodeInstruction> instructions)
        {
            return Rewrites.DropConcat(instructions, keepFirst: false);
        }
    }

    internal static class PathBuffer
    {
        internal static IEnumerable<CodeInstruction> Resize(IEnumerable<CodeInstruction> instructions)
        {
            return Rewrites.ResizePathBuffer(instructions, PerfConfig.ProjectilePathInitialCapacity.Value);
        }
    }
}
