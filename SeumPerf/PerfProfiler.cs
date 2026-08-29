using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Per-method frame timing, so "where do the milliseconds go" stops being a guess.
    ///
    /// Every instrumented method gets a Stopwatch prefix/postfix pair; the elapsed ticks are
    /// accumulated per frame and shown in the overlay. Entries marked nested live inside another
    /// instrumented method - they are a breakdown, not an addition, and only the non-nested ones
    /// are summed into the "measured" total.
    ///
    /// The profiler is not free: it adds two timestamp reads and a dictionary lookup per call, so
    /// it is off by default and the numbers it reports for very hot methods are slightly inflated.
    /// </summary>
    internal static class PerfProfiler
    {
        private struct Target
        {
            internal readonly string Spec;
            internal readonly bool Nested;

            internal Target(string spec, bool nested)
            {
                Spec = spec;
                Nested = nested;
            }
        }

        /// <summary>
        /// Top-level entries are Unity callbacks; nested ones are the interesting insides of the
        /// callbacks above them.
        /// </summary>
        private static readonly Target[] Targets =
        {
            // --- UI, the usual suspect: OnGUI runs at least twice per frame ---
            new Target("Hud:OnGUI", false),
            new Target("Hud:startLevelAimUI", true),
            new Target("Hud:inGameUI", true),
            new Target("Hud:drawScores", true),
            new Target("Hud:drawScoresSinglePlayer", true),
            new Target("Hud:drawScoreLine", true),
            new Target("Hud:drawSkullInfo", true),
            new Target("Hud:levelNameString", true),
            new Target("SeumUI:label", true),
            new Target("LeaderboardPsd:draw_background", true),
            new Target("LeaderboardsBackend:isScoreCurrentUser", true),
            new Target("LeaderboardsBackend:leaderboardForLevel", true),
            new Target("Hud:drawLevelInfoContent", true),
            new Target("Hud:Update", false),
            new Target("MainMenu:OnGUI", false),
            new Target("LevelSelector:OnGUI", false),
            new Target("LevelSelector:Update", false),

            // --- other plugins that draw or tick every frame ---
            new Target("ModLoader:OnGUI", false),
            new Target("VelocityMeter.Plugin:OnGUI", false),

            // --- the input path, for chasing high-polling-rate stalls ---
            new Target("Rewired.InputManager_Base:Update", false),
            new Target("Rewired.InputManager_Base:FixedUpdate", false),
            new Target("GameCursor:performUpdate", true),
            new Target("Game:commonLateUpdate", true),

            // --- gameplay ---
            new Target("FPSInputController:Update", false),
            new Target("FPSInputController:handleMouseLook", true),
            new Target("Projectile:generatePath", true),
            new Target("FPSInputController:LateUpdate", false),
            new Target("GameManager:FixedUpdate", false),
            new Target("CharacterMotor:performFixedUpdate", true),
            new Target("GameManager:handleCollisions", true),
            new Target("Game:Update", false),
            new Target("Game:LateUpdate", false),
            new Target("AudioManager:Update", false),
            new Target("AudioManager:LateUpdate", false),
            new Target("Projectile:Update", false),
            new Target("ProjectileTrail:LateUpdate", false),
            new Target("HandEffects:Update", false),
            new Target("Mine:Update", false),
            new Target("RingTrigger:Update", false),

            // --- rendering ---
            new Target("PortalRenderer:OnWillRenderObject", false),
            new Target("SSAOPro:OnRenderImage", false),
            new Target("Smaa.SMAA:OnRenderImage", false),
            new Target("Colorful.RadialBlur:OnRenderImage", false),
            new Target("Colorful.GaussianBlur:OnRenderImage", false),
            new Target("GhostOverlayEffect:OnRenderImage", false),
            new Target("ShadowWorldOverlayEffect:OnRenderImage", false),
            new Target("ExposureCorrection:OnRenderImage", false),
        };

        private sealed class Bucket
        {
            internal string Label;
            internal bool Nested;
            internal long Ticks;
            internal int Calls;
            internal float Milliseconds;
            internal float CallsPerFrame;
        }

        private static readonly Dictionary<MethodBase, Bucket> Buckets = new Dictionary<MethodBase, Bucket>();
        private static readonly List<Bucket> All = new List<Bucket>();
        private static readonly List<Bucket> Sorted = new List<Bucket>();

        private static readonly double TicksToMilliseconds = 1000.0 / Stopwatch.Frequency;

        private static int currentFrame = -1;

        internal static bool Active { get; private set; }

        internal static void Apply(Harmony harmony)
        {
            MethodInfo prefix = AccessTools.Method(typeof(PerfProfiler), nameof(Pre));
            MethodInfo postfix = AccessTools.Method(typeof(PerfProfiler), nameof(Post));

            int patched = 0;
            foreach (Target target in Targets)
            {
                MethodBase method = TypeResolver.ResolveMethod(target.Spec);
                if (method == null)
                {
                    if (PerfConfig.VerboseLogging.Value)
                    {
                        Plugin.Log.LogInfo($"SeumPerf profiler: '{target.Spec}' not present, skipping.");
                    }

                    continue;
                }

                if (Buckets.ContainsKey(method))
                {
                    // Two specs resolved to the same method (an inherited callback, for example).
                    continue;
                }

                try
                {
                    harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning($"SeumPerf profiler: cannot instrument '{target.Spec}': {ex.Message}");
                    continue;
                }

                Bucket bucket = new Bucket
                {
                    Label = Label(method),
                    Nested = target.Nested,
                };

                Buckets[method] = bucket;
                All.Add(bucket);
                patched++;
            }

            Active = patched > 0;
            Plugin.Log.LogInfo($"SeumPerf profiler: instrumented {patched} method(s). Overlay key: {PerfConfig.OverlayKey.Value}.");
        }

        private static string Label(MethodBase method)
        {
            string type = method.DeclaringType == null ? "?" : method.DeclaringType.Name;
            return type + "." + method.Name;
        }

        public static void Pre(out long __state)
        {
            __state = Stopwatch.GetTimestamp();
        }

        public static void Post(MethodBase __originalMethod, long __state)
        {
            long elapsed = Stopwatch.GetTimestamp() - __state;

            Bucket bucket;
            if (!Buckets.TryGetValue(__originalMethod, out bucket))
            {
                return;
            }

            int frame = Time.frameCount;
            if (frame != currentFrame)
            {
                Roll();
                currentFrame = frame;
            }

            bucket.Ticks += elapsed;
            bucket.Calls++;
        }

        /// <summary>Folds the frame just finished into a smoothed reading and resets the counters.</summary>
        private static void Roll()
        {
            for (int i = 0; i < All.Count; i++)
            {
                Bucket bucket = All[i];
                float ms = (float)(bucket.Ticks * TicksToMilliseconds);
                bucket.Milliseconds = Mathf.Lerp(bucket.Milliseconds, ms, 0.1f);
                bucket.CallsPerFrame = Mathf.Lerp(bucket.CallsPerFrame, bucket.Calls, 0.1f);
                bucket.Ticks = 0;
                bucket.Calls = 0;
            }
        }

        /// <summary>Renders the breakdown into the overlay.</summary>
        internal static void AppendReport(StringBuilder sb, int maxRows)
        {
            if (!Active)
            {
                return;
            }

            Sorted.Clear();
            float measured = 0f;
            for (int i = 0; i < All.Count; i++)
            {
                Bucket bucket = All[i];
                if (bucket.Milliseconds < 0.005f)
                {
                    continue;
                }

                Sorted.Add(bucket);
                if (!bucket.Nested)
                {
                    measured += bucket.Milliseconds;
                }
            }

            Sorted.Sort(CompareByTime);

            int rows = Mathf.Min(maxRows, Sorted.Count);
            sb.Append("\n--- ms/frame (· = inside the row above it) ---");
            for (int i = 0; i < rows; i++)
            {
                Bucket bucket = Sorted[i];
                sb.Append('\n');
                sb.Append(bucket.Nested ? "· " : "  ");
                sb.Append(Pad(bucket.Label, 27));
                sb.AppendFormat("{0,6:0.00}", bucket.Milliseconds);
                sb.AppendFormat("  x{0:0.#}", bucket.CallsPerFrame);
            }

            sb.AppendFormat("\n  measured (top level) {0:0.00} ms", measured);
        }

        private static int CompareByTime(Bucket a, Bucket b)
        {
            return b.Milliseconds.CompareTo(a.Milliseconds);
        }

        private static string Pad(string value, int width)
        {
            if (value.Length > width)
            {
                return value.Substring(0, width);
            }

            return value.PadRight(width);
        }
    }
}
