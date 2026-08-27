using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SeumDiscordRPC
{
    internal static class DiscordPatches
    {
        internal static void Apply()
        {
            // Patch each class individually instead of Harmony.PatchAll(assembly): PatchAll
            // aborts the whole batch the moment one patch throws (e.g. against a game build
            // whose IL shape differs), silently disabling every patch declared after it.
            Harmony harmony = new Harmony("SeumDiscordRPC.Patches");
            foreach (Type type in typeof(DiscordPatches).Assembly.GetTypes())
            {
                if (!Attribute.IsDefined(type, typeof(HarmonyPatch)))
                {
                    continue;
                }

                try
                {
                    new PatchClassProcessor(harmony, type).Patch();
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to apply Harmony patch '{type.FullName}': {ex}");
                }
            }
        }

        private static bool sceneTrackerCreated;

        [HarmonyPatch(typeof(Game), "commonInit")]
        private static class GameCommonInitPatch
        {
            private static void Postfix()
            {
                DiscordController.Initialize();
                if (sceneTrackerCreated)
                {
                    return;
                }

                sceneTrackerCreated = true;
                GameObject obj = new GameObject("DiscordSceneTracker");
                UnityEngine.Object.DontDestroyOnLoad(obj);
                obj.AddComponent<DiscordSceneTracker>();
            }
        }

        private static string GetLevelSymbolPrefix(int level)
        {
            if (LevelSelector.isEpLevel(level))
            {
                return LevelSelector.epLevels[0].Contains(level) ? "X" : "Y";
            }
            if (LevelSelector.isDLC1Level(level))
            {
                return "D";
            }
            if (LevelSelector.hellikuLevels.Contains(level))
            {
                return "H";
            }
            if (LevelSelector.secretLevels.Contains(level))
            {
                return "S";
            }
            return "";
        }

        private static string GetLevelNumber(int level)
        {
            if (LevelSelector.isEpLevel(level))
            {
                return (11 + LevelSelector.getGlobalLevelIndex(LevelSelector.zoneForLevel(level), LevelMetadata.levels[level].levelInZone)).ToString();
            }
            if (LevelSelector.hellikuLevels.Contains(level))
            {
                return (Array.IndexOf(LevelSelector.hellikuLevels, level) + 1).ToString();
            }
            if (LevelSelector.secretLevels.Contains(level))
            {
                return (Array.IndexOf(LevelSelector.secretLevels, level) + 1).ToString();
            }
            return (LevelSelector.getGlobalLevelIndex(LevelSelector.zoneForLevel(level), LevelMetadata.levels[level].levelInZone) - LevelSelector.zoneForLevel(level)).ToString();
        }

        private static string BuildLevelLabel(GameManager instance)
        {
            int level = Game.currentLevel;
            string label = "On " + GetLevelSymbolPrefix(level) + GetLevelNumber(level) + " " + LevelMetadata.levels[level].name;
            if (LevelSelector.currentLevelMutator != 0)
            {
                FPSInputController controller = instance.gameObject.GetComponent<FPSInputController>();
                label += $" with {controller.power}";
            }
            return label;
        }

        private static string BuildLevelDetails()
        {
            LevelData data = Game.getCurrentLevelData();
            return $"Finished: {data.timesFinished}  Dies: {data.timesDied}  Reset: {data.timesReset}  Last Time: {Math.Round(data.lastScore / 1000f, 3)}s";
        }

        [HarmonyPatch(typeof(GameManager), "performLateUpdate")]
        private static class GameManagerPerformLateUpdatePatch
        {
            private static void Postfix(GameManager __instance)
            {
                switch (__instance.gameplayState)
                {
                    case GameManager.GameplayState.START_LEVEL_AIM:
                        if (!Game.isSpeedrun() && !Game.isEndless())
                        {
                            DiscordController.UpdatePresence("Waiting", BuildLevelLabel(__instance), BuildLevelDetails(), "");
                        }
                        break;
                    case GameManager.GameplayState.FINISH_LEVEL:
                        DiscordController.UpdatePresence("Finished", BuildLevelLabel(__instance), BuildLevelDetails(), "");
                        break;
                    case GameManager.GameplayState.IN_GAME:
                        if (!Game.isEndless())
                        {
                            DiscordController.UpdatePresence("Run", BuildLevelLabel(__instance), BuildLevelDetails(), "");
                        }
                        break;
                }
            }
        }

        private static float lastEndlessDiscordUpdate;

        [HarmonyPatch(typeof(GameManager), "FixedUpdate")]
        private static class GameManagerFixedUpdatePatch
        {
            private static void Postfix(GameManager __instance)
            {
                if (__instance.gameplayState != GameManager.GameplayState.IN_GAME || !Game.isEndless())
                {
                    return;
                }

                if (Time.time - lastEndlessDiscordUpdate <= 2f)
                {
                    return;
                }

                lastEndlessDiscordUpdate = Time.time;
                int distance = (int)__instance.gameObject.transform.position.z;
                DiscordController.UpdatePresence("Endless Mode", $"Ran {distance}m", "", "");
            }
        }
    }
}
