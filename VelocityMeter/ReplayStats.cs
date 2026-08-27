using System;
using UnityEngine;

namespace VelocityMeter
{
    internal static class ReplayStats
    {
        internal static void Update()
        {
            if (Replay.replay == null || Replay.replay.frameCount < 2)
            {
                PluginState.CalculatedHSpd = 0f;
                PluginState.CalculatedVSpd = 0f;
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            if (fixedDeltaTime <= 0f)
            {
                return;
            }

            int frame = Mathf.Clamp(
                (int)(Replay.replayPlaybackTime / fixedDeltaTime),
                0,
                Replay.replay.frameCount - 2);
            Vector3 current = Replay.replay.frames[frame / 60][frame % 60].position;
            Vector3 next = Replay.replay.frames[(frame + 1) / 60][(frame + 1) % 60].position;
            Vector3 velocity = (next - current) / fixedDeltaTime;
            PluginState.CalculatedHSpd = new Vector2(velocity.x, velocity.z).magnitude;
            PluginState.CalculatedVSpd = velocity.y;

            if (PluginState.CalculatedHSpd > 12.18f && PluginState.CalculatedHSpd <= 21f)
            {
                PluginState.IsSpeeding = true;
                if (PluginState.CalculatedHSpd > PluginState.CurrentBurstMax)
                {
                    PluginState.CurrentBurstMax =
                        (float)Math.Round(PluginState.CalculatedHSpd, 3);
                }
            }
            else if (PluginState.IsSpeeding)
            {
                if (PluginState.CurrentBurstMax > 12.18f &&
                    PluginState.CurrentBurstMax <= 21f &&
                    !PluginState.SpeedPeaks.Contains(PluginState.CurrentBurstMax))
                {
                    PluginState.SpeedPeaks.Add(PluginState.CurrentBurstMax);
                    if (PluginState.SpeedPeaks.Count > 10)
                    {
                        PluginState.SpeedPeaks.RemoveAt(0);
                    }
                }

                PluginState.CurrentBurstMax = 0f;
                PluginState.IsSpeeding = false;
            }
        }

        internal static void Precalculate()
        {
            PluginState.SpeedPeaks.Clear();
            if (Replay.replay == null || Replay.replay.frameCount < 2)
            {
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            bool speeding = false;
            float burstMax = 0f;

            for (int i = 0; i < Replay.replay.frameCount - 1; i++)
            {
                Vector3 current = Replay.replay.frames[i / 60][i % 60].position;
                Vector3 next = Replay.replay.frames[(i + 1) / 60][(i + 1) % 60].position;
                float speed = new Vector2(next.x - current.x, next.z - current.z).magnitude / fixedDeltaTime;

                if (speed > 12.18f && speed <= 21f)
                {
                    speeding = true;
                    if (speed > burstMax)
                    {
                        burstMax = speed;
                    }
                }
                else if (speeding)
                {
                    float peak = (float)Math.Round(burstMax, 3);
                    if (peak > 12.18f && !PluginState.SpeedPeaks.Contains(peak))
                    {
                        PluginState.SpeedPeaks.Add(peak);
                    }

                    burstMax = 0f;
                    speeding = false;
                }
            }
        }
    }
}
