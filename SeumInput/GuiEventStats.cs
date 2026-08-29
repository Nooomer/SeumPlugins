using System.Text;
using UnityEngine;

namespace SeumInput
{
    /// <summary>
    /// Per-frame histogram of the IMGUI events reaching the game's OnGUI methods.
    ///
    /// This is the measurement the whole high-polling-rate question turns on: standing still the
    /// count should sit at two (Layout + Repaint), and every event above that is another full pass
    /// through the HUD drawing code.
    /// </summary>
    internal static class GuiEventStats
    {
        // EventType values are small and contiguous; a flat array beats a dictionary on a path
        // that runs once per event per frame.
        private const int Slots = 32;

        private static readonly int[] Counts = new int[Slots];
        private static readonly float[] Smoothed = new float[Slots];

        private static int skipped;
        private static float smoothedSkipped;
        private static float smoothedTotal;

        private static int currentFrame = -1;

        internal static float PerFrame => smoothedTotal;

        internal static void Record(EventType type)
        {
            RollIfNewFrame();

            int slot = (int)type;
            if (slot >= 0 && slot < Slots)
            {
                Counts[slot]++;
            }
        }

        internal static void RecordSkipped()
        {
            skipped++;
        }

        private static void RollIfNewFrame()
        {
            int frame = Time.frameCount;
            if (frame == currentFrame)
            {
                return;
            }

            currentFrame = frame;

            int total = 0;
            for (int i = 0; i < Slots; i++)
            {
                total += Counts[i];
                Smoothed[i] = Mathf.Lerp(Smoothed[i], Counts[i], 0.1f);
                Counts[i] = 0;
            }

            smoothedTotal = Mathf.Lerp(smoothedTotal, total, 0.1f);
            smoothedSkipped = Mathf.Lerp(smoothedSkipped, skipped, 0.1f);
            skipped = 0;
        }

        /// <summary>Appends the histogram, busiest event type first.</summary>
        internal static void AppendReport(StringBuilder sb)
        {
            sb.AppendFormat("OnGUI calls/frame {0:0.0}", smoothedTotal);

            for (int written = 0; written < 6; written++)
            {
                int best = -1;
                float bestValue = 0.05f;
                for (int i = 0; i < Slots; i++)
                {
                    if (Smoothed[i] > bestValue && !AlreadyWritten(i, written))
                    {
                        best = i;
                        bestValue = Smoothed[i];
                    }
                }

                if (best < 0)
                {
                    break;
                }

                Written[written] = best;
                sb.AppendFormat("\n   {0,-12} {1,5:0.0}", (EventType)best, Smoothed[best]);
            }

            if (InputConfig.SkipMouseMoveEvents.Value)
            {
                sb.AppendFormat("\nMouseMove dropped {0:0.0}/frame", smoothedSkipped);
            }

        }

        private static readonly int[] Written = new int[8];

        private static bool AlreadyWritten(int slot, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Written[i] == slot)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
