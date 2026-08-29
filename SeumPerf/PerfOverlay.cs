using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Measurement overlay. Added and removed on demand by <see cref="PerfRuntime"/> so it costs
    /// nothing - not even an OnGUI callback - while it is hidden.
    ///
    /// It reports the allocation rate, so it takes care not to be a meaningful part of it: the text
    /// and its measured size are rebuilt a few times a second rather than on every IMGUI event.
    /// </summary>
    internal sealed class PerfOverlay : MonoBehaviour
    {
        private const int Window = 240;
        private const int ProfilerRows = 20;

        private readonly float[] frameTimes = new float[Window];
        private readonly List<float> sortBuffer = new List<float>(Window);
        private readonly StringBuilder builder = new StringBuilder(2048);
        private readonly GUIContent content = new GUIContent();

        private int index;
        private int count;

        private long managedMemory;
        private float allocPerSecond;
        private float windowStart;
        private long windowBaseMemory;

        private int textFrame = -1;
        private float height = 80f;

        private void Update()
        {
            frameTimes[index] = Time.unscaledDeltaTime;
            index = (index + 1) % Window;
            if (count < Window)
            {
                count++;
            }

            managedMemory = GC.GetTotalMemory(false);

            if (windowStart == 0f)
            {
                windowStart = Time.unscaledTime;
                windowBaseMemory = managedMemory;
                return;
            }

            float elapsed = Time.unscaledTime - windowStart;
            if (elapsed < 1f)
            {
                return;
            }

            long delta = managedMemory - windowBaseMemory;
            // A collection inside the window makes the delta meaningless; skip that sample.
            if (delta >= 0)
            {
                allocPerSecond = delta / elapsed;
            }

            windowStart = Time.unscaledTime;
            windowBaseMemory = managedMemory;
        }

        private void Rebuild(GUIStyle style, float width)
        {
            float sum = 0f;
            float worst = 0f;
            sortBuffer.Clear();

            for (int i = 0; i < count; i++)
            {
                float t = frameTimes[i];
                sum += t;
                if (t > worst)
                {
                    worst = t;
                }

                sortBuffer.Add(t);
            }

            float average = sum / count;
            sortBuffer.Sort();
            float onePercentLow = sortBuffer[Mathf.Min(sortBuffer.Count - 1, Mathf.FloorToInt(sortBuffer.Count * 0.99f))];

            builder.Length = 0;
            builder.AppendFormat("SeumPerf   {0:0.00} ms  ({1:0} fps)", average * 1000f, 1f / average);
            builder.AppendFormat("\n1% low {0:0.00} ms  ({1:0} fps)", onePercentLow * 1000f, 1f / onePercentLow);
            builder.AppendFormat("\nworst {0:0.00} ms", worst * 1000f);
            builder.AppendFormat("\nmanaged {0:0.0} MB   alloc {1:0.0} KB/s   GC gen0 {2}",
                managedMemory / 1048576f, allocPerSecond / 1024f, GC.CollectionCount(0));

            PerfProfiler.AppendReport(builder, ProfilerRows);

            content.text = builder.ToString();

            // Measured rather than counted: the line height depends on the font that was actually
            // resolved, so multiplying an assumed row height by a line count is how the panel ended
            // up clipping its own first and last row.
            height = style.CalcHeight(content, width - 12f) + 10f;
        }

        private void OnGUI()
        {
            if (count == 0)
            {
                return;
            }

            GUIStyle style = OverlayStyle.Get();
            float width = PerfProfiler.Active ? 460f : 300f;

            if (textFrame < 0 || Time.frameCount - textFrame >= 15)
            {
                textFrame = Time.frameCount;
                Rebuild(style, width);
            }

            Rect rect = new Rect(8f, 8f, width, height);
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 6f, rect.y + 5f, rect.width - 12f, rect.height - 10f), content, style);
            GUI.color = previous;
        }
    }
}
