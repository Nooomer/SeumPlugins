using System;
using UnityEngine;

namespace SeumLimit
{
    /// <summary>
    /// Lives on its own DontDestroyOnLoad object, because every level load throws away the
    /// character and the level geometry the plugin looks at.
    /// </summary>
    internal class LimitRuntime : MonoBehaviour
    {
        private GUIStyle style;

        internal static void Create()
        {
            GameObject host = new GameObject("SeumLimitRuntime");
            host.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(host);
            host.AddComponent<LimitRuntime>();
        }

        private void Update()
        {
            try
            {
                LiveRecorder.PollInput();
                TopRuns.Tick();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Top run collection threw: " + e);
                TopRuns.Status = "Лимит: ошибка, см. лог";
            }

            try
            {
                RunReview.Tick();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Run review threw: " + e);
                RunReview.Status = "Разбор забега: ошибка, см. лог";
            }

            if (!Input.GetKeyDown(LimitConfig.DumpKey.Value))
            {
                return;
            }

            try
            {
                string path = Dumper.Write();
                Plugin.Log.LogInfo("Diagnostic dump written to " + path);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Diagnostic dump failed: " + e);
            }
        }

        private void OnGUI()
        {
            if (!LimitConfig.ShowOverlay.Value)
            {
                return;
            }

            // The limit on the aim screen, the review of the run just played on the win screen.
            if (TopRuns.Status != null && Time.frameCount - TopRuns.SeenFrame <= 2)
            {
                Draw(TopRuns.Status);
            }
            else if (RunReview.Status != null && Time.frameCount - RunReview.SeenFrame <= 2)
            {
                Draw(RunReview.Status);
            }
        }

        private void Draw(string text)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = false };
                style.normal.textColor = Color.white;
            }

            GUIContent content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);
            Rect rect = new Rect(16f, 16f, size.x + 16f, size.y + 8f);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 4f, size.x, size.y), content, style);
        }
    }
}
