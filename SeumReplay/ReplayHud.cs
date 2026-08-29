using System;
using HarmonyLib;
using UnityEngine;

namespace SeumReplay
{
    /// <summary>
    /// One extra line on the replay screen, drawn in the game's own UI space.
    ///
    /// While a replay is loading it says what is actually happening - transfer progress, which
    /// attempt this is, or that Steam gave up and the key that retries. While one is playing it
    /// says where in the run playback is and how fast it is running, which the stock screen only
    /// hints at through the position of a slider handle.
    /// </summary>
    internal static class ReplayHud
    {
        // Wider than the stock message box so a sentence fits, and just below it.
        private static readonly Rect StatusRect = new Rect(460f, 556f, 1000f, 40f);

        // Just below the "<player>'s replay" plate, which the stock screen draws over
        // ReplayPsd.playerName_rect offset by 50 - so it ends at y = 235.
        private static readonly Rect PlaybackRect = new Rect(551f, 240f, 818f, 36f);

        [HarmonyPatch(typeof(Hud), "replayUI")]
        internal static class ReplayUIPatch
        {
            private static void Postfix()
            {
                if (!ReplayConfig.StatusOverlay.Value || !GameRefs.Available)
                {
                    return;
                }

                try
                {
                    Draw();
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Replay status line threw: " + e);
                }
            }
        }

        private static void Draw()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Score score = GameRefs.SelectedReplayScore;
            string detail;
            ReplayStatus.Phase phase = ReplayStatus.Describe(score, out detail);

            switch (phase)
            {
                case ReplayStatus.Phase.Downloading:
                    DrawLine(SeumUI.responsiveMiddleCenter, StatusRect, detail, "#debd95");
                    break;

                case ReplayStatus.Phase.Failed:
                    DrawLine(SeumUI.responsiveMiddleCenter, StatusRect, detail, "#f7af6a");
                    break;

                case ReplayStatus.Phase.Ready:
                    DrawLine(SeumUI.responsiveTopCenter, PlaybackRect, PlaybackLine(), "#debd95");
                    break;
            }
        }

        private static string PlaybackLine()
        {
            Replay.ReplaySession session = Replay.replay;
            if (session == null || session.frameCount <= 0)
            {
                return null;
            }

            float total = session.frameCount * Time.fixedDeltaTime;
            float position = Mathf.Clamp(Replay.replayPlaybackTime, 0f, total);
            string speed = Replay.replaySpeed <= 0.0001f
                ? "paused"
                : Replay.replaySpeed.ToString("0.00") + "x";

            return Clock(position) + " / " + Clock(total) + "   -   " + speed;
        }

        private static string Clock(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            float rest = seconds - minutes * 60f;
            return minutes > 0
                ? minutes + ":" + rest.ToString("00.00")
                : rest.ToString("0.00") + "s";
        }

        private static void DrawLine(Matrix4x4 matrix, Rect rect, string text, string color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            SeumUI.stashAndReplaceMatrix(matrix);
            SeumUI.label(rect, text, 20, TextAnchor.MiddleCenter, color, "black");
            SeumUI.unstashMatrix();
        }
    }
}
