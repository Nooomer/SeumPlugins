using UnityEngine;

namespace SeumTimerFix
{
    /// <summary>
    /// Host for the diagnostic overlay. Its only job is to exist across scene loads and to add /
    /// remove the overlay component on key press, so that no OnGUI callback runs while the
    /// overlay is hidden. (Same layout as SeumPerf's PerfRuntime.)
    /// </summary>
    internal sealed class TimerRuntime : MonoBehaviour
    {
        private TimerOverlay overlay;

        internal static TimerRuntime Create()
        {
            GameObject host = new GameObject("SeumTimerFixRuntime");
            DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            return host.AddComponent<TimerRuntime>();
        }

        private void Awake()
        {
            if (TimerFixConfig.Overlay.Value)
            {
                ToggleOverlay();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(TimerFixConfig.OverlayKey.Value))
            {
                ToggleOverlay();
            }
        }

        private void ToggleOverlay()
        {
            if (overlay != null)
            {
                Destroy(overlay);
                overlay = null;
                return;
            }

            overlay = gameObject.AddComponent<TimerOverlay>();
        }
    }
}
