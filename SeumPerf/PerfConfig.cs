using BepInEx.Configuration;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Two kinds of switches live here, and the defaults draw the line between them.
    ///
    /// Everything under "Allocations", "Lookups" and "Redundant work" is on by default: those
    /// patches are supposed to be indistinguishable from vanilla, they only stop the game paying
    /// for the same answer twice. Everything under "Rendering" and "Quality" is off by default
    /// because it trades looks for frames, and a speedrun game is not the place to change what the
    /// player sees without being asked.
    /// </summary>
    internal static class PerfConfig
    {
        internal const int Unchanged = -1;

        internal static ConfigEntry<bool> Enabled;

        // --- allocations -------------------------------------------------------------------
        internal static ConfigEntry<bool> TrimProjectilePaths;
        internal static ConfigEntry<int> ProjectilePathInitialCapacity;
        internal static ConfigEntry<bool> ReuseTrailMeshBuffers;
        internal static ConfigEntry<bool> StripCollisionDebugStrings;
        internal static ConfigEntry<bool> GuardInputString;

        // --- lookups -----------------------------------------------------------------------
        internal static ConfigEntry<bool> CacheComponentLookups;
        internal static ConfigEntry<bool> CacheCameraMain;
        internal static ConfigEntry<bool> CacheShaderPropertyIds;
        internal static ConfigEntry<bool> CachePortalMaterials;
        internal static ConfigEntry<bool> CacheLevelData;
        internal static ConfigEntry<bool> CacheLevelNames;

        // --- redundant per-frame work -------------------------------------------------------
        internal static ConfigEntry<bool> RingTriggerOnStateChange;
        internal static ConfigEntry<bool> HellikuEarlyOut;
        internal static ConfigEntry<bool> DedupeAudioMixerWrites;
        internal static ConfigEntry<bool> CachePostEffectResourceChecks;
        internal static ConfigEntry<bool> SkipUnusedAimPath;
        internal static ConfigEntry<bool> CacheSteamUserId;

        // --- rendering (changes what you see) ----------------------------------------------
        internal static ConfigEntry<bool> DisableIdleOverlayEffects;
        internal static ConfigEntry<int> PortalTextureSize;

        // --- quality overrides (changes what you see) --------------------------------------
        internal static ConfigEntry<int> TargetFrameRate;
        internal static ConfigEntry<int> VSyncCount;
        internal static ConfigEntry<int> ShadowDistance;
        internal static ConfigEntry<int> ShadowCascades;
        internal static ConfigEntry<int> PixelLightCount;
        internal static ConfigEntry<int> ParticleRaycastBudget;
        internal static ConfigEntry<int> MasterTextureLimit;
        internal static ConfigEntry<float> LodBias;
        internal static ConfigEntry<bool> DisableAnisotropicFiltering;
        internal static ConfigEntry<bool> DisableSsao;

        // --- diagnostics --------------------------------------------------------------------
        internal static ConfigEntry<bool> Overlay;
        internal static ConfigEntry<bool> Profiler;
        internal static ConfigEntry<KeyCode> OverlayKey;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            TrimProjectilePaths = cfg.Bind("02 - Allocations", "TrimProjectilePaths", true,
                "Every projectile allocates a 4096-entry flight-path buffer (~80 KB) the moment it "
                + "spawns, and almost never fills more than a hundred entries. Start small and grow "
                + "on demand, capped at the original 4096.");

            ProjectilePathInitialCapacity = cfg.Bind("02 - Allocations", "ProjectilePathInitialCapacity", 128,
                new ConfigDescription(
                    "Initial entry count for the projectile path buffer. 128 covers a normal fireball "
                    + "flight without ever growing.",
                    new AcceptableValueRange<int>(16, 4096)));

            ReuseTrailMeshBuffers = cfg.Bind("02 - Allocations", "ReuseTrailMeshBuffers", true,
                "Projectile trails rebuild their mesh every frame and allocate four fresh arrays "
                + "each time, per live trail. Reuse per-trail buffers instead.");

            StripCollisionDebugStrings = cfg.Bind("02 - Allocations", "StripCollisionDebugStrings", true,
                "Every reported contact concatenates a debug label that is only ever read by an "
                + "unused debug dump. Keep the label, drop the concatenation.");

            GuardInputString = cfg.Bind("02 - Allocations", "GuardInputString", true,
                "Input.inputString allocates a string every frame for a cheat-code scan. Only read "
                + "it on frames where a key is actually down.");

            CacheComponentLookups = cfg.Bind("03 - Lookups", "CacheComponentLookups", true,
                "Cache the parameterless GetComponent/GetComponentInChildren/GetComponentInParent "
                + "results inside the per-frame methods that call them repeatedly.");

            CacheCameraMain = cfg.Bind("03 - Lookups", "CacheCameraMain", true,
                "Camera.main is a scene-wide tag search on Unity 2018. Projectiles and darts call it "
                + "once per instance per frame to billboard themselves.");

            CacheShaderPropertyIds = cfg.Bind("03 - Lookups", "CacheShaderPropertyIds", true,
                "Replace Material.SetColor(\"_Name\", ...) style calls in per-frame code with the "
                + "cached integer property id overloads.");

            CachePortalMaterials = cfg.Bind("03 - Lookups", "CachePortalMaterials", true,
                "Renderer.materials allocates a new array on every read. Portals read it once per "
                + "portal per rendering camera per frame.");

            CacheLevelData = cfg.Bind("03 - Lookups", "CacheLevelData", true,
                "Memoise Game.getCurrentLevelData() for the duration of a frame. On workshop levels "
                + "it walks a list and can allocate.");

            CacheLevelNames = cfg.Bind("03 - Lookups", "CacheLevelNames", true,
                "Hud.levelNameString rebuilds a level's display name from scratch on every call - "
                + "several Array.IndexOf scans, a ToUpper() and a few concatenations - although the "
                + "answer only depends on the level number. Memoise it.");

            RingTriggerOnStateChange = cfg.Bind("04 - Redundant work", "RingTriggerOnStateChange", true,
                "Ring triggers call SetActive on every object in three arrays every single frame, "
                + "even when nothing changed. Only re-apply on a state change (plus a periodic "
                + "re-assert so anything that toggles those objects behind our back still heals).");

            HellikuEarlyOut = cfg.Bind("04 - Redundant work", "HellikuEarlyOut", true,
                "Skip Helliku.Update once its one-shot material swap has already happened.");

            DedupeAudioMixerWrites = cfg.Bind("04 - Redundant work", "DedupeAudioMixerWrites", true,
                "The audio manager pushes five mixer volumes every frame whether or not they moved.");

            CachePostEffectResourceChecks = cfg.Bind("04 - Redundant work", "CachePostEffectResourceChecks", true,
                "The overlay image effects re-run their SystemInfo capability check inside "
                + "OnRenderImage on every frame. Once the material exists the answer cannot change.");

            CacheSteamUserId = cfg.Bind("04 - Redundant work", "CacheSteamUserId", true,
                "isScoreCurrentUser asks Steam for the local user's id on every leaderboard row on "
                + "every OnGUI event - measured at ~40 calls and ~3 ms per frame on the level-start "
                + "screen. The id cannot change while the game runs, so read it once.");

            SkipUnusedAimPath = cfg.Bind("04 - Redundant work", "SkipUnusedAimPath", true,
                "FPSInputController.Update runs the aim-line prediction every frame: a 100-step loop "
                + "with a Physics.SphereCastNonAlloc per step. Its result is only read while the "
                + "slow-motion aim is engaged - otherwise the trail objects are inactive and nothing "
                + "reads the path. Skip the prediction on those frames.");

            DisableIdleOverlayEffects = cfg.Bind("05 - Rendering", "DisableIdleOverlayEffects", false,
                "Disable the ghost and shadow-world overlay image effects while their intensity is "
                + "zero, so the weapon camera skips a full-screen pass (and possibly its render "
                + "texture) instead of blitting a no-op. Off by default: it assumes those shaders "
                + "are an exact identity at intensity 0, which is likely but not verified.");

            PortalTextureSize = cfg.Bind("05 - Rendering", "PortalTextureSize", Unchanged,
                new ConfigDescription(
                    "Render texture size for portal views. Portals re-render the whole scene into a "
                    + "1024x1024 target per visible portal per camera; halving it is close to a 4x "
                    + "saving on that pass. -1 leaves the game default alone.",
                    new AcceptableValueList<int>(Unchanged, 128, 256, 512, 1024)));

            TargetFrameRate = cfg.Bind("06 - Quality", "TargetFrameRate", Unchanged,
                "Application.targetFrameRate. -1 to leave alone (uncapped).");

            VSyncCount = cfg.Bind("06 - Quality", "VSyncCount", Unchanged,
                new ConfigDescription(
                    "QualitySettings.vSyncCount. -1 to leave the in-game option in charge.",
                    new AcceptableValueRange<int>(Unchanged, 4)));

            ShadowDistance = cfg.Bind("06 - Quality", "ShadowDistance", Unchanged,
                "QualitySettings.shadowDistance in metres. Usually the single biggest free GPU win. "
                + "-1 to leave alone.");

            ShadowCascades = cfg.Bind("06 - Quality", "ShadowCascades", Unchanged,
                new ConfigDescription(
                    "QualitySettings.shadowCascades. -1 to leave alone.",
                    new AcceptableValueList<int>(Unchanged, 0, 1, 2, 4)));

            PixelLightCount = cfg.Bind("06 - Quality", "PixelLightCount", Unchanged,
                "QualitySettings.pixelLightCount. Fewer per-pixel lights means fewer forward "
                + "rendering passes. -1 to leave alone.");

            ParticleRaycastBudget = cfg.Bind("06 - Quality", "ParticleRaycastBudget", Unchanged,
                "QualitySettings.particleRaycastBudget. -1 to leave alone.");

            MasterTextureLimit = cfg.Bind("06 - Quality", "MasterTextureLimit", Unchanged,
                new ConfigDescription(
                    "QualitySettings.masterTextureLimit: 0 full res, 1 half, 2 quarter. Helps when "
                    + "you are short on VRAM. -1 to leave alone.",
                    new AcceptableValueRange<int>(Unchanged, 3)));

            LodBias = cfg.Bind("06 - Quality", "LodBias", -1f,
                "QualitySettings.lodBias. Below 1 swaps to cheaper LODs sooner. -1 to leave alone.");

            DisableAnisotropicFiltering = cfg.Bind("06 - Quality", "DisableAnisotropicFiltering", false,
                "Force QualitySettings.anisotropicFiltering to Disable.");

            DisableSsao = cfg.Bind("06 - Quality", "DisableSsao", false,
                "Turn off the SSAO image effect on every camera that has one. The game already ties "
                + "it to the quality preset, this just forces it off without dropping the preset.");

            Overlay = cfg.Bind("08 - Diagnostics", "Overlay", false,
                "Draw a small frame-time / GC overlay so you can measure whether any of this helped.");

            Profiler = cfg.Bind("08 - Diagnostics", "Profiler", false,
                "Time individual methods (HUD drawing, the aim prediction, image effects, the other "
                + "plugins' OnGUI) and show a per-frame breakdown in the overlay. Costs a little "
                + "performance itself, so leave it off unless you are measuring.");

            OverlayKey = cfg.Bind("08 - Diagnostics", "OverlayKey", KeyCode.F10,
                "Toggles the overlay at runtime.");

            VerboseLogging = cfg.Bind("08 - Diagnostics", "VerboseLogging", false,
                "Log every individual patch as it is applied.");
        }
    }
}
