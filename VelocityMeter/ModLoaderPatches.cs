using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Rewired;
using Steamworks;
using UnityEngine;

namespace VelocityMeter
{
    internal static class ModLoaderPatches
    {
        internal static void Apply()
        {
            // Patch each class individually instead of Harmony.PatchAll(assembly): PatchAll
            // aborts the whole batch the moment one patch throws (e.g. a transpiler that
            // doesn't match the currently loaded game build), silently disabling every
            // patch declared after it. Isolating failures keeps the rest of the mod working.
            Harmony harmony = new Harmony("VelocityMeter.ModLoader");
            foreach (Type type in typeof(ModLoaderPatches).Assembly.GetTypes())
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

        // Fields that are private on the original game classes. Cached once instead of
        // going through Harmony's Traverse on every call (several of these run per-frame).
        private static readonly AccessTools.FieldRef<Projectile, GameObject> ProjectileTrailParticlesRef =
            AccessTools.FieldRefAccess<Projectile, GameObject>("trailParticles");
        private static readonly AccessTools.FieldRef<Projectile, bool> ProjectileInitialTrailParticleSpawnedRef =
            AccessTools.FieldRefAccess<Projectile, bool>("initialTrailParticleSpawned");
        private static readonly AccessTools.FieldRef<SpawnedPlatform, ParticleSystem> SpawnedPlatformRocksRef =
            AccessTools.FieldRefAccess<SpawnedPlatform, ParticleSystem>("rocksPS");
        private static readonly AccessTools.FieldRef<SpawnedPlatform, ParticleSystem> SpawnedPlatformSmokeRef =
            AccessTools.FieldRefAccess<SpawnedPlatform, ParticleSystem>("smokePS");
        private static readonly AccessTools.FieldRef<Hud, FPSInputController> HudInputControllerRef =
            AccessTools.FieldRefAccess<Hud, FPSInputController>("inputController");
        private static readonly AccessTools.FieldRef<IntroSplash, float> IntroSplashTimePerLogoRef =
            AccessTools.FieldRefAccess<IntroSplash, float>("timePerLogo");
        private static readonly AccessTools.FieldRef<FPSInputController, CharacterMotor> FpsMotorRef =
            AccessTools.FieldRefAccess<FPSInputController, CharacterMotor>("motor");
        private static readonly AccessTools.FieldRef<FPSInputController, GameManager> FpsGameManagerRef =
            AccessTools.FieldRefAccess<FPSInputController, GameManager>("gameManager");
        private static readonly AccessTools.FieldRef<FPSInputController, TransformInterpolation> FpsViewInterpolationRef =
            AccessTools.FieldRefAccess<FPSInputController, TransformInterpolation>("viewInterpolation");
        private static readonly AccessTools.FieldRef<FPSInputController, float> FpsTimeInAirRef =
            AccessTools.FieldRefAccess<FPSInputController, float>("timeInAir");
        private static readonly AccessTools.FieldRef<SteamDownloadRequest, CallResult<LeaderboardScoresDownloaded_t>> LeaderboardScoresResultRef =
            AccessTools.FieldRefAccess<SteamDownloadRequest, CallResult<LeaderboardScoresDownloaded_t>>("leaderboardScoresResult");
        private static readonly MethodInfo LeaderboardScoresDownloadedMethod =
            AccessTools.Method(typeof(SteamDownloadRequest), "leaderboardScoresDownloaded");

        [HarmonyPatch(typeof(Replay), "simulateStep")]
        private static class ReplaySimulateStepPatch
        {
            private static void Prefix()
            {
                // Original modded IL checks KeyCode value 116, which is KeyCode.T (not KeyCode.U).
                if (Input.GetKeyDown(KeyCode.T))
                {
                    ReplayBridge.ToggleTrail();
                }

                if (Input.GetKeyDown(KeyCode.K))
                {
                    PluginState.ShowInputOverlay = !PluginState.ShowInputOverlay;
                }
            }

            private static void Postfix()
            {
                ReplayStats.Update();
                ReplayInputOverlay.Update();
            }
        }

        [HarmonyPatch(typeof(Replay), "playFromStart")]
        private static class ReplayPlayFromStartPatch
        {
            private static void Prefix()
            {
                PluginState.CurrentBurstMax = 0f;
                PluginState.IsSpeeding = false;
                ReplayStats.Precalculate();
                ReplayInputOverlay.Reset();
            }
        }

        [HarmonyPatch(typeof(IntroSplash), "Start")]
        private static class IntroSplashStartPatch
        {
            // This is where the original mod called ModLoader.Initialize() from - by this
            // point GameSettings and other game-static state ModLoader's constructor reads
            // are already set up, unlike at Plugin.Awake() time.
            //
            // The mod also skipped the studio splash logos here by setting timePerLogo to 0
            // instead of the vanilla 2 seconds. IntroSplash.Update() divides its elapsed timer
            // by timePerLogo to decide when to fade/advance to the next logo; with 0 the ratio
            // is instantly "past the end", so both logos clear on the first eligible frame and
            // the menu scene activates right away instead of waiting ~2s per logo.
            private static void Postfix(IntroSplash __instance)
            {
                ModLoader.Initialize();
                IntroSplashTimePerLogoRef(__instance) = 0f;
            }
        }

        [HarmonyPatch(typeof(Game), "Update")]
        private static class GameUpdatePatch
        {
            // Mirrors the modded Game.OnGUI(), which tracked the player's Y position every frame.
            private static void Postfix(Game __instance)
            {
                if (__instance.character != null)
                {
                    PluginState.YAxis = __instance.character.transform.position.y;
                }
            }
        }

        private static readonly FieldInfo GameEffectsField = AccessTools.Field(typeof(Game), "effects");

        // OnParticles/DLCSKY: the mod destroyed the level's per-character environment
        // effect / cleared the skybox right after the vanilla method set them up. Doing it
        // as a Postfix here (rather than intercepting the Instantiate call inside the
        // original method) keeps this independent of the exact internal IL shape.
        private static void SuppressEnvironmentEffect(Game game)
        {
            if (!PluginState.OnParticles || game.character == null)
            {
                return;
            }

            FPSInputController controller = game.character.GetComponent<FPSInputController>();
            if (controller != null && controller.environmentEffect != null)
            {
                UnityEngine.Object.Destroy(controller.environmentEffect);
                controller.environmentEffect = null;
            }
        }

        [HarmonyPatch(typeof(Game), "changeLevel")]
        private static class GameChangeLevelPatch
        {
            private static void Postfix(Game __instance, bool addEnvironment)
            {
                SuppressEnvironmentEffect(__instance);
                if (addEnvironment && PluginState.DlcSky)
                {
                    RenderSettings.skybox = null;
                }
            }
        }

        [HarmonyPatch(typeof(Game), "initEndlessMode")]
        private static class GameInitEndlessModePatch
        {
            private static void Postfix(Game __instance)
            {
                SuppressEnvironmentEffect(__instance);
            }
        }

        [HarmonyPatch(typeof(Game), "syncEffectPrefab")]
        private static class GameSyncEffectPrefabPatch
        {
            // OnEffect: the mod destroyed the shared level-effects prefab right after this
            // (static) method instantiated and assigned it to Game.effects.
            private static void Postfix()
            {
                if (!PluginState.OnEffect)
                {
                    return;
                }

                GameObject effects = (GameObject)GameEffectsField.GetValue(null);
                if (effects != null)
                {
                    UnityEngine.Object.Destroy(effects);
                    GameEffectsField.SetValue(null, null);
                }
            }
        }

        [HarmonyPatch(typeof(Game), "suggestedEnvrionmentForLevel")]
        private static class GameSuggestedEnvironmentForLevelPatch
        {
            // Full replacement of this pure, side-effect-free method (translated from the
            // original IL) with the two DLCNOTHEME branches the mod added, swapping the
            // DLC "Hell7"/environments[1] theme for a plainer "Hell6"/environments[5] one.
            private static bool Prefix(int level, ref string __result)
            {
                if (Game.startedFrom == StartedFrom.WORKSHOP && Game.currentLevel == 0xc5)
                {
                    __result = "Hell" + LevelMetadata.levels[Game.currentLevel].environmentId;
                    return false;
                }

                string result = LevelMetadata.environments[0];
                int zone = LevelMetadata.levels[level].zone - 1;
                if (zone >= 0)
                {
                    result = LevelSelector.zoneEnvironment[zone];
                }

                foreach (int[][] dlcGroup in LevelSelector.dlcLevels)
                {
                    foreach (int[] dlcLevelSet in dlcGroup)
                    {
                        if (Array.IndexOf(dlcLevelSet, level) != -1)
                        {
                            result = PluginState.DlcNoTheme ? "Hell6" : "Hell7";
                        }
                    }
                }

                if (Array.IndexOf(LevelSelector.secretLevels, level) != -1 || Array.IndexOf(LevelSelector.hellikuLevels, level) != -1)
                {
                    result = LevelMetadata.environments[PluginState.DlcNoTheme ? 5 : 1];
                }

                LevelMetadata.Record record = LevelMetadata.levels[level];
                if (record.environmentId != -1)
                {
                    result = LevelMetadata.environments[record.environmentId];
                }

                __result = result;
                return false;
            }
        }

        private static Color CrosshairColor()
        {
            if (ModLoader.CrossColor1) return new Color(1f, 1f, 1f, 1f);
            if (ModLoader.CrossColor2) return new Color(1f, 0f, 1f, 0f);
            if (ModLoader.CrossColor3) return new Color(1f, 1f, 1f, 0f);
            if (ModLoader.CrossColor4) return new Color(1f, 1f, 0f, 0f);
            if (ModLoader.CrossColor5) return new Color(1f, 0f, 0f, 1f);
            if (ModLoader.CrossColor6) return new Color(1f, 0f, 0f, 0f);
            if (ModLoader.CrossColor7) return new Color(1f, 0.8f, 5.8f, 0.682f);
            if (ModLoader.CrossColor8) return new Color(1f, 0.125f, 0.811f, 0.784f);
            if (ModLoader.CrossColor9) return new Color(1f, 0.811f, 0.427f, 0.125f);
            return Color.white;
        }

        private static bool IsLocal(CodeInstruction instruction, int index)
        {
            if (instruction.operand is LocalBuilder local)
            {
                return local.LocalIndex == index;
            }

            return instruction.operand is byte byteIndex && byteIndex == index ||
                   instruction.operand is int intIndex && intIndex == index;
        }

        private static Color GetTrailColor(float fade)
        {
            if (ModLoader.TrailRed) return new Color(1f, 0f, 0f, fade);
            if (ModLoader.TrailYellow) return new Color(1f, 1f, 0f, fade);
            if (ModLoader.TrailBlue) return new Color(0f, 0f, 1f, fade);
            if (ModLoader.TrailGreen) return new Color(0f, 1f, 0f, fade);
            if (ModLoader.TrailBlack) return new Color(0f, 0f, 0f, fade);
            if (ModLoader.TrailMagenta) return new Color(1f, 0f, 1f, fade);
            if (ModLoader.TrailWhite) return new Color(1f, 1f, 1f, fade);
            if (ModLoader.TrailLerp1) return Color.Lerp(Color.green, Color.magenta, Mathf.PingPong(Time.time, 1f));
            if (ModLoader.TrailLerp2) return Color.Lerp(Color.blue, Color.magenta, Mathf.PingPong(Time.time, 1f));
            if (ModLoader.TrailLerp3) return Color.Lerp(Color.blue, Color.green, Mathf.PingPong(Time.time, 1f));
            if (ModLoader.TrailLerp4) return Color.Lerp(Color.gray, Color.cyan, Mathf.PingPong(Time.time, 1f));
            if (ModLoader.TrailLerp5) return Color.Lerp(Color.yellow, Color.magenta, Mathf.PingPong(Time.time, 1f));
            if (ModLoader.TrailLerp6) return Color.Lerp(Color.green, Color.yellow, Mathf.PingPong(Time.time, 1f));
            return new Color(1f, 1f, 1f, fade);
        }

        [HarmonyPatch(typeof(Crosshair), "draw")]
        private static class CrosshairDrawPatch
        {
            private static bool Prefix(float offsetX, float offsetY)
            {
                int index = GameSettings.settings.crosshairIndex;
                Color color = CrosshairColor();

                switch (index)
                {
                    case 0: CrosshairPsd.draw_crosshair1(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 1: CrosshairPsd.draw_crosshair2(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 2: CrosshairPsd.draw_crosshair3(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 3: CrosshairPsd.draw_crosshair4(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 4: CrosshairPsd.draw_crosshair5(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 5: CrosshairPsd.draw_crosshair6(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 6: CrosshairPsd.draw_crosshair7(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 7: CrosshairPsd.draw_crosshair8(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 8: CrosshairPsd.draw_crosshair9(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                    case 9: CrosshairPsd.draw_crosshair10(offsetX, offsetY, color.r, color.g, color.b, color.a); break;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(FPSInputController), "Awake")]
        private static class FpsInputControllerAwakeResetPatch
        {
            // The mod resets the angle-set toggle here, in Awake() - which only fires once, when
            // a new FPSInputController is created (i.e. a new level's character), not on a same-
            // level restart. We only ported the "apply" half of this feature onto
            // RestartLevelCallback; this is the other half, so the toggle actually turns itself
            // off on level transitions instead of carrying over indefinitely.
            private static void Postfix()
            {
                ModLoader.enableAngleSet = false;
                ModLoader.targetAngleX = 0f;
                ModLoader.targetAngleY = 0f;
            }
        }

        [HarmonyPatch(typeof(FPSInputController), "RestartLevelCallback")]
        private static class FpsInputControllerRestartLevelCallbackPatch
        {
            // The modded game applied the custom angle here, not in Awake(): Awake() only
            // ever fires once (object creation), while RestartLevelCallback() runs on every
            // level restart/respawn - the point that actually needs the angle re-applied.
            // Awake() itself resets enableAngleSet/targetAngleX/Y to false/0/0, so patching
            // Awake can never work: by the time our postfix ran there, the flag was already off.
            private static void Postfix(FPSInputController __instance)
            {
                PluginState.BufferedFire1 = false;
                PluginState.BufferedFire2 = false;

                if (!ModLoader.enableAngleSet)
                {
                    return;
                }

                Quaternion rotation = Quaternion.Euler(0f, ModLoader.targetAngleX, 0f);
                __instance.transform.rotation = rotation;
                __instance.originalRotation = rotation;
                __instance.rotationY = ModLoader.targetAngleY;
                __instance.cameraRoot.transform.localEulerAngles = new Vector3(-ModLoader.targetAngleY, 0f, 0f);
            }
        }

        [HarmonyPatch(typeof(FPSInputController), "Update")]
        private static class FpsInputControllerUpdatePatch
        {
            private static void Postfix(FPSInputController __instance)
            {
                if (PluginState.NoFireballs && __instance.characterView != null && __instance.characterView.trailHit != null)
                {
                    __instance.characterView.trailHit.SetActive(false);
                }

                BufferFireInputDuringAim(__instance);
            }
        }

        // The mod buffers a Fire1/Fire2 press made while still in START_LEVEL_AIM, because the
        // level-start transition (changeStateNextFrame) lands one frame later, by which point
        // the original Rewired button-down edge has already passed - so without buffering, the
        // same press that starts the level never registers as a fire/mutator-use input. This is
        // why pressing the mutator button "starts the level but doesn't activate the mutator."
        private static void BufferFireInputDuringAim(FPSInputController controller)
        {
            if (!Game.inFocus || DebugHud.isCapturingMouse())
            {
                return;
            }

            GameManager manager = FpsGameManagerRef(controller);
            if (manager == null || manager.gameplayState != GameManager.GameplayState.START_LEVEL_AIM)
            {
                return;
            }

            if (controller.player.GetButtonDown(4) || controller.player.GetButtonDown(37))
            {
                PluginState.BufferedFire1 = true;
            }

            if (controller.player.GetButtonDown(5) || controller.player.GetButtonDown(38))
            {
                PluginState.BufferedFire2 = true;
            }
        }

        // Consumed here rather than by transpiling handleHandInput's own fire1Down/fire2Down
        // computation: bracket the call with a scoped override so Rewired.Player.GetButtonDown
        // reports the buffered press as "just pressed" only for calls made during this one
        // method call, then clear it. Avoids IL surgery on a method whose exact shape isn't
        // guaranteed to match the currently loaded game build.
        private static bool overrideFire1;
        private static bool overrideFire2;

        [HarmonyPatch(typeof(FPSInputController), "handleHandInput")]
        private static class FpsHandleHandInputBufferPatch
        {
            private static void Prefix()
            {
                overrideFire1 = PluginState.BufferedFire1;
                overrideFire2 = PluginState.BufferedFire2;
                PluginState.BufferedFire1 = false;
                PluginState.BufferedFire2 = false;
            }

            private static void Postfix()
            {
                overrideFire1 = false;
                overrideFire2 = false;
            }
        }

        [HarmonyPatch(typeof(Player), "GetButtonDown", new[] { typeof(int) })]
        private static class RewiredPlayerGetButtonDownPatch
        {
            private static void Postfix(int actionId, ref bool __result)
            {
                if (__result)
                {
                    return;
                }

                if (actionId == 4 && overrideFire1)
                {
                    __result = true;
                }
                else if (actionId == 5 && overrideFire2)
                {
                    __result = true;
                }
            }
        }

        private static bool NameMatchesAny(string name, params string[] fragments)
        {
            string lower = name.ToLower();
            foreach (string fragment in fragments)
            {
                if (lower.Contains(fragment))
                {
                    return true;
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(FPSInputController), "spawnProjectile")]
        private static class FpsSpawnProjectileNoFireballsPatch
        {
            // Translated from the modded IL: on top of the animation swap (handled by the
            // Transpiler patch above), NoFireballs also strips every visual off the spawned
            // projectile - renderers, trail renderers, particle systems, lights, and any
            // GameObject-typed field whose name looks VFX-related. Without this, only the
            // trail-particle system was suppressed and everything else (glow, light, any
            // baked-in trail renderer) kept rendering normally.
            private static void Postfix(Projectile __result)
            {
                if (!PluginState.NoFireballs || __result == null)
                {
                    return;
                }

                __result.initialTrailParticleSpawnTime = float.MaxValue;

                foreach (Transform child in __result.GetComponentsInChildren<Transform>(true))
                {
                    if (NameMatchesAny(child.gameObject.name, "trail", "particle", "spark", "ember", "fire"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                foreach (Renderer renderer in __result.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = false;
                }

                foreach (TrailRenderer trailRenderer in __result.GetComponentsInChildren<TrailRenderer>(true))
                {
                    trailRenderer.Clear();
                    trailRenderer.time = 0f;
                    trailRenderer.enabled = false;
                }

                foreach (ParticleSystem particles in __result.GetComponentsInChildren<ParticleSystem>(true))
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    particles.gameObject.SetActive(false);
                }

                foreach (Light light in __result.GetComponentsInChildren<Light>(true))
                {
                    light.enabled = false;
                }

                foreach (FieldInfo field in __result.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.FieldType == typeof(GameObject) &&
                        NameMatchesAny(field.Name, "hit", "explo", "impact", "trail", "particle", "spark", "fire", "glow", "effect", "fx"))
                    {
                        field.SetValue(__result, null);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FPSInputController), "performFixedUpdate")]
        private static class FpsInputControllerPerformFixedUpdatePatch
        {
            // The mod added a dedicated branch for gameplayState == REPLAY that keeps feeding
            // the camera's TransformInterpolation every fixed step (addFixedUpdate). Without
            // it, replay playback falls into the same path as "no input yet" (initTo), which
            // snaps the camera instead of smoothing it between fixed steps - this is what
            // reads as replay motion stuttering/looking low-framerate, especially once
            // slow-motion stretches each fixed step over more real time.
            private static bool Prefix(FPSInputController __instance)
            {
                GameManager manager = FpsGameManagerRef(__instance);
                if (manager == null || manager.gameplayState != GameManager.GameplayState.REPLAY)
                {
                    return true;
                }

                TransformInterpolation.addFixedUpdate(FpsViewInterpolationRef(__instance), __instance.cameraRoot.transform);

                CharacterMotor motor = FpsMotorRef(__instance);
                if (motor != null && !motor.grounded && motor.enabled && Physics.gravity.y < 0f)
                {
                    FpsTimeInAirRef(__instance) += Time.deltaTime;
                }
                else
                {
                    FpsTimeInAirRef(__instance) = 0f;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PlatformDestroyAnimation), "Start")]
        private static class PlatformDestroyAnimationStartPatch
        {
            private static bool Prefix(PlatformDestroyAnimation __instance)
            {
                if (!PluginState.NoBlockBreak)
                {
                    return true;
                }

                UnityEngine.Object.Destroy(__instance.gameObject);
                return false;
            }
        }

        [HarmonyPatch(typeof(ProjectileTrail), "Start")]
        private static class ProjectileTrailStartPatch
        {
            // Deliberate departure from the mod: the user doesn't want the glowing projectile
            // trail at all, regardless of NoFireballs. Patching ProjectileTrail's own Start()
            // (rather than Projectile.Start()) guarantees the MeshRenderer/component already
            // exist, since this runs right after the base method just created them - no
            // component-lifecycle timing risk. Disabling the component itself also stops
            // LateUpdate from running at all, so no mesh geometry gets built in the first place.
            private static void Postfix(ProjectileTrail __instance)
            {
                __instance.enabled = false;

                MeshRenderer trailRenderer = __instance.GetComponent<MeshRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.enabled = false;
                }
            }
        }

        // Only for Projectile.FixedUpdate's one-time initial spawn (see below) - do NOT
        // reset initialTrailParticleSpawned here. It used to be reset unconditionally on
        // every call, which made the vanilla "!initialTrailParticleSpawned" guard in
        // FixedUpdate pass again on the very next tick, so the game kept Instantiating a
        // fresh particle system every FixedUpdate just for us to destroy it immediately -
        // an Instantiate/Destroy loop for every live projectile, worst during replay
        // slow-motion where FixedUpdate runs many times per rendered frame.
        private static void SuppressInitialTrailParticles(Projectile projectile)
        {
            if (!PluginState.NoFireballs)
            {
                return;
            }

            GameObject particles = ProjectileTrailParticlesRef(projectile);
            if (particles != null)
            {
                particles.SetActive(false);
                UnityEngine.Object.Destroy(particles);
                ProjectileTrailParticlesRef(projectile) = null;
            }
        }

        private static void StopAttackAnimation(Animation animation)
        {
            if (!PluginState.NoFireballs)
            {
                animation.Stop();
            }
        }

        private static bool PlayAttackAnimation(Animation animation, string clip, PlayMode mode)
        {
            return !PluginState.NoFireballs && animation.Play(clip, mode);
        }

        private static AnimationState PlayQueuedAttackAnimation(Animation animation, string clip, QueueMode mode)
        {
            return PluginState.NoFireballs ? null : animation.PlayQueued(clip, mode);
        }

        [HarmonyPatch(typeof(Projectile), "addPathPoint")]
        private static class ProjectileAddPathPointPatch
        {
            // Full replacement, translated directly from the modded IL, instead of a
            // Transpiler (a previous attempt crashed Harmony's IL verifier on startup - see
            // git history). The real logic never destroys trailParticles: on a path split it
            // always disables the emission module of the CURRENT trail particles, and only
            // skips spawning a replacement when NoFireballs is on - so the existing trail
            // burns out naturally instead of vanishing/flickering.
            private static bool Prefix(Projectile __instance, float time, Vector3 position, bool split, ref bool __result)
            {
                Path path = __instance.path;
                if (path.count >= path.points.Length)
                {
                    Debug.Log("NOTE(Tomislav): Too many positions tracked... This should have been destoryed long ago! Killing the projectile");
                    __result = false;
                    return false;
                }

                path.points[path.count].timeStamp = time;
                path.points[path.count].position = position;
                path.points[path.count].split = split;
                path.count++;

                if (split)
                {
                    GameObject trailParticles = ProjectileTrailParticlesRef(__instance);
                    if (trailParticles != null && ProjectileInitialTrailParticleSpawnedRef(__instance))
                    {
                        ParticleSystem.EmissionModule emission = trailParticles.GetComponent<ParticleSystem>().emission;
                        emission.enabled = false;

                        if (!PluginState.NoFireballs)
                        {
                            GameObject newTrailParticles = UnityEngine.Object.Instantiate(__instance.trailParticlesPrefab, position, Quaternion.identity);
                            newTrailParticles.transform.parent = Game.transientLevelContainer.transform;
                            ProjectileTrailParticlesRef(__instance) = newTrailParticles;
                        }
                    }
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(FPSInputController), "spawnProjectile")]
        private static class FpsSpawnProjectilePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);
                MethodInfo stop = AccessTools.Method(typeof(Animation), "Stop", Type.EmptyTypes);
                MethodInfo play = AccessTools.Method(typeof(Animation), "Play", new[] { typeof(string), typeof(PlayMode) });
                MethodInfo playQueued = AccessTools.Method(typeof(Animation), "PlayQueued", new[] { typeof(string), typeof(QueueMode) });
                MethodInfo stopReplacement = AccessTools.Method(typeof(ModLoaderPatches), "StopAttackAnimation");
                MethodInfo playReplacement = AccessTools.Method(typeof(ModLoaderPatches), "PlayAttackAnimation");
                MethodInfo playQueuedReplacement = AccessTools.Method(typeof(ModLoaderPatches), "PlayQueuedAttackAnimation");

                for (int i = 0; i < code.Count; i++)
                {
                    MethodInfo method = code[i].operand as MethodInfo;
                    if (method == stop)
                    {
                        code[i].operand = stopReplacement;
                    }
                    else if (method == play)
                    {
                        code[i].operand = playReplacement;
                    }
                    else if (method == playQueued)
                    {
                        code[i].operand = playQueuedReplacement;
                    }
                }

                return code;
            }
        }

        [HarmonyPatch(typeof(Projectile), "FixedUpdate")]
        private static class ProjectileFixedUpdatePatch
        {
            private static void Postfix(Projectile __instance)
            {
                SuppressInitialTrailParticles(__instance);
            }
        }

        [HarmonyPatch(typeof(SpawnedPlatform), "Update")]
        private static class SpawnedPlatformUpdatePatch
        {
            private static void Postfix(SpawnedPlatform __instance)
            {
                if (!PluginState.NoFireballs)
                {
                    return;
                }

                ParticleSystem rocks = SpawnedPlatformRocksRef(__instance);
                ParticleSystem smoke = SpawnedPlatformSmokeRef(__instance);
                if (rocks != null)
                {
                    rocks.Stop();
                }

                if (smoke != null)
                {
                    smoke.Stop();
                }
            }
        }

        [HarmonyPatch(typeof(Hud), "restartGame")]
        private static class HudRestartGamePatch
        {
            private static bool Prefix()
            {
                return !PluginState.RestartBlockEnabled || PluginState.RestartBlockTimer <= 0f;
            }
        }

        [HarmonyPatch(typeof(Hud), "Update")]
        private static class HudUpdatePatch
        {
            // restartBlockTimer/restartBlockEnabled don't exist on the original Hud class -
            // they were added by the mod. Ported here as PluginState so restartGame() has
            // something real to check instead of always reading 0 through Traverse.
            private static void Postfix(Hud __instance)
            {
                if (PluginState.RestartBlockTimer > 0f)
                {
                    PluginState.RestartBlockTimer -= Time.deltaTime;
                }

                FPSInputController inputController = HudInputControllerRef(__instance);
                CharacterMotor motor = inputController != null ? inputController.GetComponent<CharacterMotor>() : null;
                if (motor == null)
                {
                    return;
                }

                float verticalSpeed = motor.movement.velocity.y;
                if (verticalSpeed >= 15f && verticalSpeed <= 30f)
                {
                    PluginState.RestartBlockTimer = 1f;
                }
            }
        }

        [HarmonyPatch(typeof(SteamDownloadRequest), "downloadLeaderboardEntries")]
        private static class SteamDownloadRequestDownloadLeaderboardEntriesPatch
        {
            // NumberStartS doesn't exist on the original class - the mod only ever used it to
            // shift the range window for the "Global" leaderboard request here. Full
            // replacement (translated from the original IL) rather than trying to inject just
            // the offset, since the whole range/request-type selection happens inline.
            private static bool Prefix(SteamDownloadRequest __instance, SteamLeaderboard_t leaderboard)
            {
                ELeaderboardDataRequest requestType = (ELeaderboardDataRequest)0;
                int rangeStart = PluginState.NumberStartS;
                int rangeEnd = 49 + PluginState.NumberStartS;

                if (__instance.type == 1)
                {
                    requestType = (ELeaderboardDataRequest)1;
                    rangeStart = -3;
                    rangeEnd = 20;
                }
                else if (__instance.type == 2)
                {
                    requestType = (ELeaderboardDataRequest)2;
                }

                SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(leaderboard, requestType, rangeStart, rangeEnd);
                var callback = (CallResult<LeaderboardScoresDownloaded_t>.APIDispatchDelegate)
                    Delegate.CreateDelegate(
                        typeof(CallResult<LeaderboardScoresDownloaded_t>.APIDispatchDelegate),
                        __instance,
                        LeaderboardScoresDownloadedMethod);

                CallResult<LeaderboardScoresDownloaded_t> result = CallResult<LeaderboardScoresDownloaded_t>.Create(callback);
                LeaderboardScoresResultRef(__instance) = result;
                result.Set(call, null);

                return false;
            }
        }

        [HarmonyPatch(typeof(ProjectileTrail), "LateUpdate")]
        private static class ProjectileTrailLateUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> code = new List<CodeInstruction>(instructions);
                ConstructorInfo colorConstructor = AccessTools.Constructor(typeof(Color), new[]
                {
                    typeof(float), typeof(float), typeof(float), typeof(float)
                });
                MethodInfo getTrailColor = AccessTools.Method(typeof(ModLoaderPatches), "GetTrailColor");

                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode != OpCodes.Ldloca_S || !IsLocal(code[i], 17))
                    {
                        continue;
                    }

                    int constructorIndex = -1;
                    for (int j = i + 1; j < code.Count && j <= i + 18; j++)
                    {
                        if (code[j].opcode == OpCodes.Call && code[j].operand is MethodInfo method && method == colorConstructor)
                        {
                            constructorIndex = j;
                            break;
                        }
                    }

                    if (constructorIndex < 0)
                    {
                        continue;
                    }

                    code.RemoveRange(i, constructorIndex - i + 1);
                    code.InsertRange(i, new[]
                    {
                        new CodeInstruction(OpCodes.Ldc_R4, 1f),
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldloc_S, 12),
                        new CodeInstruction(OpCodes.Sub),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Div),
                        new CodeInstruction(OpCodes.Sub),
                        new CodeInstruction(OpCodes.Call, getTrailColor),
                        new CodeInstruction(OpCodes.Stloc_S, 17)
                    });
                    break;
                }

                return code;
            }
        }
    }
}
