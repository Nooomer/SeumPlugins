using UnityEngine;

namespace VelocityMeter
{
    internal static class ReplayBridge
    {
        private static LineRenderer pathLine;

        internal static void ToggleTrail()
        {
            PluginState.ShowTrail = !PluginState.ShowTrail;
            if (PluginState.ShowTrail)
            {
                CreateTrail();
            }
            else
            {
                DestroyTrail();
            }
        }

        internal static void DestroyTrail()
        {
            if (pathLine != null)
            {
                Object.Destroy(pathLine.gameObject);
                pathLine = null;
            }
        }

        internal static void CreateTrail()
        {
            DestroyTrail();

            if (Replay.replay == null || Replay.replay.frameCount == 0)
            {
                return;
            }

            GameObject trailObject = new GameObject("ReplayTrail");
            pathLine = trailObject.AddComponent<LineRenderer>();
            pathLine.material = new Material(Shader.Find("Sprites/Default"));
            pathLine.startColor = new Color(0f, 1f, 1f, 0.4f);
            pathLine.endColor = new Color(0f, 0.5f, 1f, 0.1f);
            pathLine.startWidth = 0.1f;
            pathLine.endWidth = 0.1f;
            pathLine.useWorldSpace = true;

            int frameCount = Replay.replay.frameCount;
            Vector3[] positions = new Vector3[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                positions[i] = Replay.replay.frames[i / 60][i % 60].position;
            }

            pathLine.positionCount = positions.Length;
            pathLine.SetPositions(positions);
        }
    }
}
