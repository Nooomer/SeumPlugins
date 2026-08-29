using System.Collections;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// Splits the frame into three phases so a stall can be located when it is provably not in any
    /// script.
    ///
    /// The profiler in SeumPerf accounts for the time spent inside managed methods. When it reports
    /// 0.53 ms of a 11 ms frame, the remaining 95% is engine time, and the next question is which
    /// part of the engine. Three timestamps bracket it:
    ///
    ///   Update        -> LateUpdate        the script phase
    ///   LateUpdate    -> WaitForEndOfFrame rendering (culling, draw call submission, image effects)
    ///   EndOfFrame    -> next Update       present, the Windows message pump, vsync waiting
    ///
    /// The mouse messages a high polling rate device produces are drained by the message pump, so
    /// if the spikes land in the third bucket the cost is in the engine's input handling or in the
    /// present call, and no Harmony patch can reach it. If they land in rendering, it is the GPU
    /// or the driver.
    ///
    /// This component's Update is not guaranteed to be the frame's first, nor its LateUpdate the
    /// last, so the script figure is a lower bound. The other two brackets are what matter.
    /// </summary>
    internal sealed class FramePhaseProbe : MonoBehaviour
    {
        private const int Window = 240;

        private static readonly double TicksToMilliseconds = 1000.0 / Stopwatch.Frequency;

        private readonly float[] scripts = new float[Window];
        private readonly float[] render = new float[Window];
        private readonly float[] present = new float[Window];

        private int index;
        private int count;

        private long updateStamp;
        private long lateStamp;
        private long endOfFrameStamp;
        private bool haveEndOfFrame;
        private bool haveLate;

        private void OnEnable()
        {
            StartCoroutine(EndOfFrameLoop());
        }

        private void Update()
        {
            long now = Stopwatch.GetTimestamp();

            if (haveEndOfFrame)
            {
                // Everything between the end of last frame's rendering and the start of this
                // frame's scripts: present, the message pump, and any waiting.
                present[index] = (float)((now - endOfFrameStamp) * TicksToMilliseconds);

                index = (index + 1) % Window;
                if (count < Window)
                {
                    count++;
                }
            }

            updateStamp = now;
        }

        private void LateUpdate()
        {
            lateStamp = Stopwatch.GetTimestamp();
            haveLate = true;
            scripts[index] = (float)((lateStamp - updateStamp) * TicksToMilliseconds);
        }

        private IEnumerator EndOfFrameLoop()
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            while (true)
            {
                yield return wait;

                endOfFrameStamp = Stopwatch.GetTimestamp();
                if (haveLate)
                {
                    render[index] = (float)((endOfFrameStamp - lateStamp) * TicksToMilliseconds);
                }

                haveEndOfFrame = true;
            }
        }

        internal void AppendReport(StringBuilder sb)
        {
            if (count == 0)
            {
                sb.Append("\nframe phases: no samples yet");
                return;
            }

            Summarise(scripts, out float scriptsAvg, out float scriptsMax);
            Summarise(render, out float renderAvg, out float renderMax);
            Summarise(present, out float presentAvg, out float presentMax);

            sb.AppendFormat("\n{0,-16}{1,8}{2,9}", "phase", "avg ms", "worst ms");
            sb.AppendFormat("\n{0,-16}{1,8:0.00}{2,9:0.00}", "  scripts", scriptsAvg, scriptsMax);
            sb.AppendFormat("\n{0,-16}{1,8:0.00}{2,9:0.00}", "  render", renderAvg, renderMax);
            sb.AppendFormat("\n{0,-16}{1,8:0.00}{2,9:0.00}", "  present+pump", presentAvg, presentMax);

            AppendStalls(sb);
        }

        /// <summary>
        /// Stalls per second rather than a single worst-case number. Comparing two configurations
        /// by their worst frame is noisy - a rate over the whole window is what actually tells you
        /// whether a change helped.
        /// </summary>
        private void AppendStalls(StringBuilder sb)
        {
            float threshold = InputConfig.StallThresholdMs.Value;
            int stalls = 0;
            float totalMs = 0f;

            for (int i = 0; i < count; i++)
            {
                float frame = scripts[i] + render[i] + present[i];
                totalMs += frame;
                if (frame > threshold)
                {
                    stalls++;
                }
            }

            float seconds = totalMs / 1000f;
            sb.AppendFormat("\nstalls >{0:0}ms: {1} in {2:0.0}s = {3:0.0}/s",
                threshold, stalls, seconds, seconds > 0.01f ? stalls / seconds : 0f);
        }

        private void Summarise(float[] samples, out float average, out float worst)
        {
            float sum = 0f;
            worst = 0f;
            for (int i = 0; i < count; i++)
            {
                float value = samples[i];
                sum += value;
                if (value > worst)
                {
                    worst = value;
                }
            }

            average = sum / count;
        }
    }
}
