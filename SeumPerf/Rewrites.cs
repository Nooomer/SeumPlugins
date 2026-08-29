using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Generic IL rewrites. Each one is a pure substitution: an expensive call is swapped for a
    /// helper with the identical stack signature, so the surrounding method keeps its exact shape
    /// (branch targets, try/catch blocks and locals are all untouched).
    /// </summary>
    internal static class Rewrites
    {
        private static readonly MethodInfo CompGO = AccessTools.Method(typeof(Cached), nameof(Cached.CompGO));
        private static readonly MethodInfo CompC = AccessTools.Method(typeof(Cached), nameof(Cached.CompC));
        private static readonly MethodInfo ChildGO = AccessTools.Method(typeof(Cached), nameof(Cached.ChildGO));
        private static readonly MethodInfo ChildC = AccessTools.Method(typeof(Cached), nameof(Cached.ChildC));
        private static readonly MethodInfo ParentGO = AccessTools.Method(typeof(Cached), nameof(Cached.ParentGO));
        private static readonly MethodInfo ParentC = AccessTools.Method(typeof(Cached), nameof(Cached.ParentC));

        private static readonly MethodInfo CameraMainGetter = AccessTools.PropertyGetter(typeof(Camera), "main");
        private static readonly MethodInfo CameraMainReplacement = AccessTools.Method(typeof(Cached), nameof(Cached.MainCamera));

        private static readonly MethodInfo RendererMaterialsGetter = AccessTools.PropertyGetter(typeof(Renderer), "materials");
        private static readonly MethodInfo RendererMaterialsReplacement = AccessTools.Method(typeof(Cached), nameof(Cached.Materials));

        private static readonly MethodInfo InputStringGetter = AccessTools.PropertyGetter(typeof(Input), "inputString");
        private static readonly MethodInfo InputStringReplacement = AccessTools.Method(typeof(Cached), nameof(Cached.InputString));

        private static readonly MethodInfo ConcatTwo =
            AccessTools.Method(typeof(string), "Concat", new[] { typeof(string), typeof(string) });

        /// <summary>Rewrites the parameterless GetComponent family onto the cached lookups.</summary>
        internal static IEnumerable<CodeInstruction> CacheComponentLookups(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ins in instructions)
            {
                MethodInfo target = MatchComponentLookup(ins);
                yield return target == null ? ins : Retarget(ins, target);
            }
        }

        private static MethodInfo MatchComponentLookup(CodeInstruction ins)
        {
            if (ins.opcode != OpCodes.Call && ins.opcode != OpCodes.Callvirt)
            {
                return null;
            }

            MethodInfo mi = ins.operand as MethodInfo;
            if (mi == null || !mi.IsGenericMethod || mi.GetParameters().Length != 0)
            {
                return null;
            }

            bool onGameObject = mi.DeclaringType == typeof(GameObject);
            bool onComponent = mi.DeclaringType == typeof(Component);
            if (!onGameObject && !onComponent)
            {
                return null;
            }

            Type arg = mi.GetGenericArguments()[0];
            // GetComponent<T> also accepts interfaces; the cache is constrained to Component.
            if (!typeof(Component).IsAssignableFrom(arg))
            {
                return null;
            }

            MethodInfo replacement;
            switch (mi.Name)
            {
                case "GetComponent":
                    replacement = onGameObject ? CompGO : CompC;
                    break;
                case "GetComponentInChildren":
                    replacement = onGameObject ? ChildGO : ChildC;
                    break;
                case "GetComponentInParent":
                    replacement = onGameObject ? ParentGO : ParentC;
                    break;
                default:
                    return null;
            }

            return replacement.MakeGenericMethod(arg);
        }

        internal static IEnumerable<CodeInstruction> CacheCameraMain(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceCalls(instructions, CameraMainGetter, CameraMainReplacement);
        }

        internal static IEnumerable<CodeInstruction> CacheRendererMaterials(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceCalls(instructions, RendererMaterialsGetter, RendererMaterialsReplacement);
        }

        internal static IEnumerable<CodeInstruction> GuardInputString(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceCalls(instructions, InputStringGetter, InputStringReplacement);
        }

        private static readonly MethodInfo GeneratePath = AccessTools.Method(typeof(Projectile), "generatePath");
        private static readonly MethodInfo GuardedGeneratePath = AccessTools.Method(typeof(AimPath), nameof(AimPath.Generate));

        /// <summary>
        /// Routes the aim-line prediction through <see cref="AimPath.Generate"/>, which skips the
        /// ~100 sphere casts on the frames where nothing reads the result. See AimPath for why that
        /// is unobservable.
        /// </summary>
        internal static IEnumerable<CodeInstruction> GuardAimPath(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceCalls(instructions, GeneratePath, GuardedGeneratePath);
        }

        /// <summary>
        /// Rewrites Material.SetColor("name", ...) and friends onto cached property ids. The
        /// helpers take the material as their first parameter, so the instance call and the static
        /// call consume exactly the same stack.
        /// </summary>
        internal static IEnumerable<CodeInstruction> CacheShaderPropertyIds(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ins in instructions)
            {
                MethodInfo target = MatchMaterialStringCall(ins);
                yield return target == null ? ins : Retarget(ins, target);
            }
        }

        private static MethodInfo MatchMaterialStringCall(CodeInstruction ins)
        {
            if (ins.opcode != OpCodes.Call && ins.opcode != OpCodes.Callvirt)
            {
                return null;
            }

            MethodInfo mi = ins.operand as MethodInfo;
            if (mi == null || mi.DeclaringType != typeof(Material))
            {
                return null;
            }

            ParameterInfo[] ps = mi.GetParameters();
            if (ps.Length == 0 || ps[0].ParameterType != typeof(string))
            {
                return null;
            }

            switch (mi.Name)
            {
                case "SetColor":
                    return ps.Length == 2 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.SetColor)) : null;
                case "SetFloat":
                    return ps.Length == 2 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.SetFloat)) : null;
                case "SetVector":
                    return ps.Length == 2 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.SetVector)) : null;
                case "SetTexture":
                    return ps.Length == 2 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.SetTexture)) : null;
                case "SetMatrix":
                    return ps.Length == 2 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.SetMatrix)) : null;
                case "HasProperty":
                    return ps.Length == 1 ? AccessTools.Method(typeof(ShaderIds), nameof(ShaderIds.HasProperty)) : null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Turns a two-argument string.Concat into a pick-one-side no-op. Used to strip the debug
        /// label concatenations out of the collision reporting path, which runs per contact per
        /// physics step and whose result is only ever read by an unused debug dump.
        /// </summary>
        internal static IEnumerable<CodeInstruction> DropConcat(IEnumerable<CodeInstruction> instructions, bool keepFirst)
        {
            MethodInfo replacement = keepFirst
                ? AccessTools.Method(typeof(Cached), nameof(Cached.First))
                : AccessTools.Method(typeof(Cached), nameof(Cached.Second));

            return ReplaceCalls(instructions, ConcatTwo, replacement);
        }

        /// <summary>Rewrites the array size baked into Path's constructor.</summary>
        internal static IEnumerable<CodeInstruction> ResizePathBuffer(IEnumerable<CodeInstruction> instructions, int newSize)
        {
            foreach (CodeInstruction ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4 && ins.operand is int && (int)ins.operand == 4096)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, newSize)
                    {
                        labels = ins.labels,
                        blocks = ins.blocks,
                    };
                    continue;
                }

                yield return ins;
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceCalls(
            IEnumerable<CodeInstruction> instructions, MethodInfo from, MethodInfo to)
        {
            foreach (CodeInstruction ins in instructions)
            {
                bool isCall = ins.opcode == OpCodes.Call || ins.opcode == OpCodes.Callvirt;
                yield return isCall && SameMethod(ins.operand as MethodInfo, from) ? Retarget(ins, to) : ins;
            }
        }

        /// <summary>
        /// Structural comparison rather than reference equality: the game assembly binds several of
        /// these through netstandard type forwards, so the MethodInfo Harmony hands back is not
        /// guaranteed to be the very same object typeof(...) produces here.
        /// </summary>
        private static bool SameMethod(MethodInfo candidate, MethodInfo expected)
        {
            if (candidate == null || expected == null)
            {
                return false;
            }

            if (candidate == expected)
            {
                return true;
            }

            if (candidate.Name != expected.Name || candidate.DeclaringType != expected.DeclaringType)
            {
                return false;
            }

            ParameterInfo[] a = candidate.GetParameters();
            ParameterInfo[] b = expected.GetParameters();
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].ParameterType != b[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        private static CodeInstruction Retarget(CodeInstruction ins, MethodInfo target)
        {
            return new CodeInstruction(OpCodes.Call, target)
            {
                labels = ins.labels,
                blocks = ins.blocks,
            };
        }
    }
}
