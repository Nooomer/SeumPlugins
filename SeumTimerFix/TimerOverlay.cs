using System;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SeumTimerFix
{
    /// <summary>
    /// Timer-drift diagnostics. Added and removed on demand by <see cref="TimerRuntime"/> so it
    /// costs nothing - not even an OnGUI callback - while it is hidden.
    ///
    /// Two numbers matter:
    ///
    ///  1. Session drift: (double)Time.fixedTime vs the exact tick grid FixedTick * fixedDeltaTime.
    ///     Unity's float32 fixedTime accumulates one rounded addition per physics step, so this is
    ///     the accumulated error of the physical clock itself - the timer bug, made visible.
    ///  2. Run drift: what the vanilla level time (Time.fixedTime - levelSpawnTime + offset, all
    ///     float32) would read right now, minus the tick-exact value SeumTimerFix hands the game.
    ///     It starts at zero when the run captures its spawn time and grows with the float error.
    /// </summary>
    internal sealed class TimerOverlay : MonoBehaviour
    {
        private const int RebuildEvery = 15;

        private readonly StringBuilder builder = new StringBuilder(512);
        private readonly GUIContent content = new GUIContent();

        private int textFrame = -1;
        private float height = 70f;

        private GameManager manager;

        private static readonly AccessTools.FieldRef<GameManager, float> LevelSpawnTimeRef =
            AccessTools.FieldRefAccess<GameManager, float>("levelSpawnTime");

        private static readonly AccessTools.FieldRef<GameManager, float> LevelFinishTimeOffsetRef =
            AccessTools.FieldRefAccess<GameManager, float>("levelFinishTimeOffset");

        // The tick grid starts at plugin load while Time.fixedTime starts at game start, so the
        // raw difference carries an arbitrary constant. Snap the first sample to zero drift.
        private bool baseCaptured;
        private double baseOffset;

        private double sessionDrift;
        private double sessionDriftMin;
        private double sessionDriftMax;
        private double sessionDriftAbsMax;

        private long lastSpawnTick = -1;
        private double runDrift;
        private double runDriftAbsMax;

        private void Update()
        {
            if (manager == null)
            {
                manager = FindObjectOfType<GameManager>();
            }

            double fixedDeltaTime = Time.fixedDeltaTime;
            double grid = (double)TimerPatches.FixedTick * fixedDeltaTime;
            double offset = (double)Time.fixedTime - grid;
            if (!baseCaptured)
            {
                baseOffset = offset;
                baseCaptured = true;
            }

            sessionDrift = (offset - baseOffset) * 1000.0;
            if (sessionDrift < sessionDriftMin)
            {
                sessionDriftMin = sessionDrift;
            }

            if (sessionDrift > sessionDriftMax)
            {
                sessionDriftMax = sessionDrift;
            }

            double abs = Math.Abs(sessionDrift);
            if (abs > sessionDriftAbsMax)
            {
                sessionDriftAbsMax = abs;
            }

            // A new spawn anchor means a new run: restart the per-run counter.
            if (TimerPatches.SpawnTick != lastSpawnTick)
            {
                lastSpawnTick = TimerPatches.SpawnTick;
                runDriftAbsMax = 0.0;
            }

            double spawn = manager != null ? (double)LevelSpawnTimeRef(manager) : 0.0;
            double finishOffset = manager != null ? (double)LevelFinishTimeOffsetRef(manager) : 0.0;
            double vanilla = (double)Time.fixedTime - spawn + finishOffset;

            long ticks = TimerPatches.FixedTick - TimerPatches.SpawnTick;
            if (ticks < 0)
            {
                ticks = 0;
            }

            double exact = (double)ticks * fixedDeltaTime + finishOffset;
            runDrift = (vanilla - exact) * 1000.0;
            double runAbs = Math.Abs(runDrift);
            if (runAbs > runDriftAbsMax)
            {
                runDriftAbsMax = runAbs;
            }
        }

        private void Rebuild(GUIStyle style, float width)
        {
            builder.Length = 0;
            builder.AppendFormat("SeumTimerFix   (F6 - toggle)");
            builder.AppendFormat("\nsession {0:0.0} s   float ULP {1:0.000} ms",
                Time.fixedTime, UlpOf(Time.fixedTime) * 1000f);
            builder.AppendFormat("\nfixedTime drift {0:+0.000;-0.000; 0.000} ms   min {1:0.000}   max {2:0.000}   |max| {3:0.000}",
                sessionDrift, sessionDriftMin, sessionDriftMax, sessionDriftAbsMax);
            builder.AppendFormat("\nrun drift {0:+0.000;-0.000; 0.000} ms   |max| {1:0.000}",
                runDrift, runDriftAbsMax);

            content.text = builder.ToString();
            height = style.CalcHeight(content, width - 12f) + 10f;
        }

        private void OnGUI()
        {
            GUIStyle style = TimerOverlayStyle.Get();
            const float width = 480f;

            if (textFrame < 0 || Time.frameCount - textFrame >= RebuildEvery)
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

        /// <summary>
        /// The distance between two adjacent float32 values at the given magnitude - the grid
        /// step that Time.fixedTime is quantized to once it has grown this large.
        /// </summary>
        private static float UlpOf(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            int exponent = Mathf.FloorToInt(Mathf.Log(value, 2f));
            return Mathf.Pow(2f, exponent - 23);
        }
    }
}
