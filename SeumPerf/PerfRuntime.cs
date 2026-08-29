using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeumPerf
{
    /// <summary>
    /// The runtime half of the mod: the things that have to react to the game rather than replace
    /// a method - cache invalidation on level change, the opt-in quality overrides, and disabling
    /// image effects while they are idle.
    /// </summary>
    internal sealed class PerfRuntime : MonoBehaviour
    {
        private GhostOverlayEffect ghost;
        private ShadowWorldOverlayEffect shadowWorld;
        private bool effectsSearched;

        private int nextQualityFrame;
        private PerfOverlay overlay;

        internal static PerfRuntime Create()
        {
            GameObject host = new GameObject("SeumPerfRuntime");
            DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            return host.AddComponent<PerfRuntime>();
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (PerfConfig.Overlay.Value)
            {
                ToggleOverlay();
            }
        }

        private void Start()
        {
            // Deferred to Start on purpose: BepInEx loads plugins in sequence, and SeumPerf comes
            // before VelocityMeter in that order, so at Awake time the other plugins' assemblies
            // are not necessarily loaded yet and the profiler could not resolve their OnGUI.
            if (PerfConfig.Profiler.Value && Plugin.HarmonyInstance != null)
            {
                PerfProfiler.Apply(Plugin.HarmonyInstance);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Every cache in the mod is keyed on instance ids that do not survive a level change.
            CacheRegistry.ClearAll();
            ghost = null;
            shadowWorld = null;
            effectsSearched = false;
            nextQualityFrame = 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown(PerfConfig.OverlayKey.Value))
            {
                ToggleOverlay();
            }

            // The game re-applies its own quality preset whenever the options screen is touched,
            // so the overrides are re-asserted rather than set once.
            if (Time.frameCount >= nextQualityFrame)
            {
                nextQualityFrame = Time.frameCount + 60;
                ApplyQualityOverrides();
            }
        }

        /// <summary>
        /// The overlay lives on its own component so that a mod about saving frames does not leave
        /// an OnGUI callback - and the IMGUI event pump that comes with it - running when nobody
        /// asked for one.
        /// </summary>
        private void ToggleOverlay()
        {
            if (overlay != null)
            {
                Destroy(overlay);
                overlay = null;
                return;
            }

            overlay = gameObject.AddComponent<PerfOverlay>();
        }

        private void LateUpdate()
        {
            if (!PerfConfig.DisableIdleOverlayEffects.Value)
            {
                return;
            }

            if (!effectsSearched)
            {
                ghost = FindObjectOfType<GhostOverlayEffect>();
                shadowWorld = FindObjectOfType<ShadowWorldOverlayEffect>();
                effectsSearched = ghost != null || shadowWorld != null;
            }

            // Both effects are driven by an intensity the gameplay code sets every frame; at zero
            // they are a full-screen blit that changes nothing. Toggling `enabled` also lets the
            // weapon camera skip its intermediate render texture when nothing else needs one.
            if (ghost != null)
            {
                bool wanted = ghost.intensity > 0.0001f;
                if (ghost.enabled != wanted)
                {
                    ghost.enabled = wanted;
                }
            }

            if (shadowWorld != null)
            {
                bool wanted = shadowWorld.intensity > 0.0001f || shadowWorld.activeEffect > 0.0001f;
                if (shadowWorld.enabled != wanted)
                {
                    shadowWorld.enabled = wanted;
                }
            }
        }

        private void ApplyQualityOverrides()
        {
            if (PerfConfig.TargetFrameRate.Value != PerfConfig.Unchanged
                && Application.targetFrameRate != PerfConfig.TargetFrameRate.Value)
            {
                Application.targetFrameRate = PerfConfig.TargetFrameRate.Value;
            }

            if (PerfConfig.VSyncCount.Value != PerfConfig.Unchanged
                && QualitySettings.vSyncCount != PerfConfig.VSyncCount.Value)
            {
                QualitySettings.vSyncCount = PerfConfig.VSyncCount.Value;
            }

            if (PerfConfig.ShadowDistance.Value != PerfConfig.Unchanged)
            {
                QualitySettings.shadowDistance = PerfConfig.ShadowDistance.Value;
            }

            if (PerfConfig.ShadowCascades.Value != PerfConfig.Unchanged)
            {
                QualitySettings.shadowCascades = PerfConfig.ShadowCascades.Value;
            }

            if (PerfConfig.PixelLightCount.Value != PerfConfig.Unchanged)
            {
                QualitySettings.pixelLightCount = PerfConfig.PixelLightCount.Value;
            }

            if (PerfConfig.ParticleRaycastBudget.Value != PerfConfig.Unchanged)
            {
                QualitySettings.particleRaycastBudget = PerfConfig.ParticleRaycastBudget.Value;
            }

            if (PerfConfig.MasterTextureLimit.Value != PerfConfig.Unchanged)
            {
                QualitySettings.masterTextureLimit = PerfConfig.MasterTextureLimit.Value;
            }

            if (PerfConfig.LodBias.Value > 0f)
            {
                QualitySettings.lodBias = PerfConfig.LodBias.Value;
            }

            if (PerfConfig.DisableAnisotropicFiltering.Value
                && QualitySettings.anisotropicFiltering != AnisotropicFiltering.Disable)
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            }

            if (PerfConfig.PortalTextureSize.Value != PerfConfig.Unchanged)
            {
                ApplyPortalTextureSize(PerfConfig.PortalTextureSize.Value);
            }

            if (PerfConfig.DisableSsao.Value)
            {
                SSAOPro[] ssao = FindObjectsOfType<SSAOPro>();
                for (int i = 0; i < ssao.Length; i++)
                {
                    if (ssao[i].enabled)
                    {
                        ssao[i].enabled = false;
                    }
                }
            }
        }

        private static readonly HarmonyLib.AccessTools.FieldRef<PortalRenderer, int> PortalTextureSizeRef =
            HarmonyLib.AccessTools.FieldRefAccess<PortalRenderer, int>("m_TextureSize");

        private static void ApplyPortalTextureSize(int size)
        {
            PortalRenderer[] portals = FindObjectsOfType<PortalRenderer>();
            for (int i = 0; i < portals.Length; i++)
            {
                if (PortalTextureSizeRef(portals[i]) != size)
                {
                    // PortalRenderer recreates its render texture whenever the requested size stops
                    // matching the one it built, so this needs no extra bookkeeping.
                    PortalTextureSizeRef(portals[i]) = size;
                }
            }
        }
    }
}
