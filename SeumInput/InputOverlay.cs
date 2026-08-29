using System.Text;
using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// Shows the IMGUI event histogram and the latency settings currently in force.
    ///
    /// This overlay is itself an OnGUI, so it is deliberately cheap: the text and its measured size
    /// are rebuilt a few times a second, not on every event it is trying to measure.
    /// </summary>
    internal sealed class InputOverlay : MonoBehaviour
    {
        private const float Width = 400f;

        private readonly StringBuilder builder = new StringBuilder(512);
        private readonly GUIContent content = new GUIContent();

        private int textFrame = -1;
        private float height = 80f;

        private void Rebuild(GUIStyle style)
        {
            builder.Length = 0;
            builder.Append("SeumInput\n");

            GuiEventStats.AppendReport(builder);

            builder.AppendFormat("\nmaxQueuedFrames {0}   {1}",
                QualitySettings.maxQueuedFrames, Screen.fullScreenMode);

            FramePhaseProbe phases = InputRuntime.Instance == null ? null : InputRuntime.Instance.Phases;
            if (phases != null)
            {
                phases.AppendReport(builder);
            }

            if (OrderProbe.Active)
            {
                float ratio = OrderProbe.RewiredFirstRatio;
                builder.Append(ratio < 0f
                    ? "\nupdate order: no samples yet"
                    : string.Format("\nRewired before game input: {0:0}%  ({1} frames)",
                        ratio * 100f, OrderProbe.Samples));
            }

            content.text = builder.ToString();

            // Measured rather than counted: the line height depends on the font that was actually
            // resolved, so multiplying an assumed row height by a line count is how the panel ended
            // up clipping its own first and last row.
            height = style.CalcHeight(content, Width - 12f) + 10f;
        }

        private void OnGUI()
        {
            GUIStyle style = OverlayStyle.Get();

            if (textFrame < 0 || Time.frameCount - textFrame >= 15)
            {
                textFrame = Time.frameCount;
                Rebuild(style);
            }

            Rect rect = new Rect(8f, 320f, Width, height);
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 6f, rect.y + 5f, rect.width - 12f, rect.height - 10f), content, style);
            GUI.color = previous;
        }
    }
}
