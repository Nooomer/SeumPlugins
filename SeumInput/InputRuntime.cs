using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// Applies the latency settings and owns the diagnostics overlay.
    /// </summary>
    internal sealed class InputRuntime : MonoBehaviour
    {
        private InputOverlay overlay;
        internal FramePhaseProbe Phases { get; private set; }
        internal static InputRuntime Instance { get; private set; }
        private int nextApplyFrame;
        private WindowMode lastWindowMode = WindowMode.Unchanged;
        private bool windowModeApplied;
        private bool rewiredReported;

        internal static InputRuntime Create()
        {
            GameObject host = new GameObject("SeumInputRuntime");
            DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            return host.AddComponent<InputRuntime>();
        }

        private void Awake()
        {
            Instance = this;

            if (InputConfig.ProbeFramePhases.Value)
            {
                Phases = gameObject.AddComponent<FramePhaseProbe>();
            }

            if (InputConfig.Overlay.Value)
            {
                ToggleOverlay();
            }
        }

        private void Start()
        {
            // Rewired finishes its own initialisation during the first frames, so the report and
            // the order probe are set up here rather than in the plugin's Awake.
            if (InputConfig.ProbeUpdateOrder.Value && Plugin.HarmonyInstance != null)
            {
                OrderProbe.Apply(Plugin.HarmonyInstance);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(InputConfig.OverlayKey.Value))
            {
                ToggleOverlay();
            }

            if (!rewiredReported && InputConfig.LogRewiredReport.Value && Time.frameCount > 30)
            {
                rewiredReported = true;
                RewiredReport.Log();
            }

            if (Time.frameCount < nextApplyFrame)
            {
                return;
            }

            nextApplyFrame = Time.frameCount + 60;
            ApplyLatencySettings();
        }

        private void ApplyLatencySettings()
        {
            // Re-asserted periodically: the game calls QualitySettings.SetQualityLevel whenever the
            // options screen is touched, and the quality level carries its own maxQueuedFrames.
            int queued = InputConfig.MaxQueuedFrames.Value;
            if (queued != InputConfig.Unchanged && QualitySettings.maxQueuedFrames != queued)
            {
                QualitySettings.maxQueuedFrames = queued;
            }

            // The window mode is not re-asserted, only applied on startup and when the setting
            // changes, so the in-game options screen keeps control of the window afterwards.
            WindowMode wanted = InputConfig.FullScreenMode.Value;
            if (wanted != WindowMode.Unchanged && (!windowModeApplied || wanted != lastWindowMode))
            {
                windowModeApplied = true;
                lastWindowMode = wanted;
                ApplyWindowMode(wanted);
            }
        }

        private static void ApplyWindowMode(WindowMode mode)
        {
            FullScreenMode target;
            switch (mode)
            {
                case WindowMode.ExclusiveFullScreen:
                    target = UnityEngine.FullScreenMode.ExclusiveFullScreen;
                    break;
                case WindowMode.FullScreenWindow:
                    target = UnityEngine.FullScreenMode.FullScreenWindow;
                    break;
                case WindowMode.MaximizedWindow:
                    target = UnityEngine.FullScreenMode.MaximizedWindow;
                    break;
                case WindowMode.Windowed:
                    target = UnityEngine.FullScreenMode.Windowed;
                    break;
                default:
                    return;
            }

            if (Screen.fullScreenMode != target)
            {
                Screen.fullScreenMode = target;
                Plugin.Log.LogInfo($"SeumInput: window mode set to {target}.");
            }
        }

        /// <summary>
        /// The overlay is its own component so that a plugin about IMGUI cost does not leave an
        /// extra OnGUI callback running when nobody is looking at it.
        /// </summary>
        private void ToggleOverlay()
        {
            if (overlay != null)
            {
                Destroy(overlay);
                overlay = null;
                return;
            }

            overlay = gameObject.AddComponent<InputOverlay>();
        }
    }
}
