using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// One prefix on every OnGUI the game ships, doing two jobs at once: counting the IMGUI events
    /// that actually arrive, and dropping the ones the game cannot use.
    ///
    /// Both live in a single prefix deliberately - a prefix that returns false stops the remaining
    /// prefixes from running, so a separate counting patch could be silently skipped by the
    /// filtering one and under-report exactly the events we care about.
    /// </summary>
    internal static class GuiEvents
    {
        /// <summary>
        /// Only classes that are actually reachable in the shipped game. The demo and sample
        /// scenes that came with the asset packs are left alone - patching them buys nothing and
        /// only widens the blast radius.
        /// </summary>
        private static readonly string[] Targets =
        {
            "Hud",
            "MainMenu",
            "LevelSelector",
            "LevelEditor",
            "IntroSplash",
            "DLCIntroOutro",
            "EnterName",
            "Strip",
            "AudioVisualEditor",
        };

        private static readonly Assembly GameAssembly = typeof(Hud).Assembly;

        internal static int PatchedCount { get; private set; }

        internal static void Apply(Harmony harmony)
        {
            MethodInfo prefix = AccessTools.Method(typeof(GuiEvents), nameof(Prefix));

            foreach (string typeName in Targets)
            {
                Type type = GameAssembly.GetType(typeName, throwOnError: false);
                if (type == null)
                {
                    continue;
                }

                MethodBase onGui = AccessTools.Method(type, "OnGUI");
                if (onGui == null)
                {
                    continue;
                }

                try
                {
                    harmony.Patch(onGui, new HarmonyMethod(prefix));
                    PatchedCount++;
                    if (InputConfig.VerboseLogging.Value)
                    {
                        Plugin.Log.LogInfo($"SeumInput: hooked {typeName}.OnGUI");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"SeumInput: cannot hook {typeName}.OnGUI: {ex.Message}");
                }
            }

            Plugin.Log.LogInfo($"SeumInput: hooked {PatchedCount} OnGUI method(s).");
        }

        public static bool Prefix()
        {
            Event current = Event.current;
            if (current == null)
            {
                return true;
            }

            EventType type = current.type;
            GuiEventStats.Record(type);

            if (InputConfig.SkipMouseMoveEvents.Value && type == EventType.MouseMove)
            {
                GuiEventStats.RecordSkipped();
                return false;
            }

            return true;
        }
    }
}
