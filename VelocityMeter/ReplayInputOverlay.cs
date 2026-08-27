using UnityEngine;

namespace VelocityMeter
{
    // Replay data has no raw per-frame button state: ReplayFullFrame only stores
    // position/rotation/forwardVelocity, and the rest of the input picture has to come from
    // Replay.events (a handful of discrete ReplayEvent records). Each indicator here has a
    // different reliability tier as a result:
    //  - Forward/back is exact: forwardVelocity is the raw Rewired axis value captured at
    //    record time (FPSInputController.directionVector.z, despite the misleading field
    //    name), not a physics quantity.
    //  - Fire1/Fire2 flashes are exact: they come straight off Replay.events, reported once
    //    per shot.
    //  - Gravity Flip is exact and level-triggered, not edge-triggered: the game reports a
    //    GRAVITY_POWER event every tick the ability is held, with data=1 while held and
    //    data=2 while released (see FPSInputController's ability-handling method), so this
    //    tracks "currently held" rather than flashing once.
    //  - Left/right strafe isn't recorded at all (only directionVector.z made it into the
    //    replay format, not .x). It's inferred from lateral ACCELERATION (change in local-x
    //    velocity between consecutive frames) rather than raw lateral velocity: velocity alone
    //    also lights up during any sideways momentum carried into a fall (walked off a ledge,
    //    got launched, etc.), since that momentum doesn't come from a currently-held key.
    //    Acceleration only shows up while something is actively steering the character
    //    sideways, which in practice means grounded movement - still a heuristic, not a real
    //    key read, but far less prone to false positives from airborne drift.
    //  - Jump has no recorded signal whatsoever - there's no JUMP entry in ReplayEvent, and
    //    the closest event (UNGROUND) is reported from five different code paths (real jumps,
    //    explosions, gravity flips, arbitrary velocity overrides) that are indistinguishable
    //    from each other once they're just a type/data/timestamp triple in the replay. Instead
    //    this looks for the one thing a real jump always produces that ordinary falling never
    //    does: an instantaneous upward jump in vertical velocity in a single tick (gravity
    //    only ever changes vertical velocity gradually). It will also light up for launch pads
    //    or knockback, which is a real limitation, not a bug.
    internal static class ReplayInputOverlay
    {
        private const float ForwardAxisThreshold = 0.35f;
        private const float StrafeAccelThreshold = 15f; // m/s^2 of lateral velocity change per tick
        private const float JumpImpulseThreshold = 6f; // m/s of upward velocity gained in one tick

        private static bool moveForward;
        private static bool moveBack;
        private static bool moveLeft;
        private static bool moveRight;
        private static bool gravityActive;
        private static float fire1FlashUntil;
        private static float fire2FlashUntil;
        private static float jumpFlashUntil;
        private static int nextEventIndex;
        private static float previousLocalVelocityX;
        private static float previousVerticalVelocity;
        private static bool hasPreviousVelocity;

        private static GUIStyle keyStyle;
        private static GUIStyle timerStyle;

        internal static void Reset()
        {
            moveForward = false;
            moveBack = false;
            moveLeft = false;
            moveRight = false;
            gravityActive = false;
            fire1FlashUntil = 0f;
            fire2FlashUntil = 0f;
            jumpFlashUntil = 0f;
            nextEventIndex = 0;
            previousLocalVelocityX = 0f;
            previousVerticalVelocity = 0f;
            hasPreviousVelocity = false;
        }

        internal static void Update()
        {
            moveForward = false;
            moveBack = false;
            moveLeft = false;
            moveRight = false;

            if (Replay.replay == null || Replay.replay.frameCount < 2)
            {
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

            Replay.ReplayFullFrame current = Replay.replay.frames[frame / 60][frame % 60];
            Replay.ReplayFullFrame next = Replay.replay.frames[(frame + 1) / 60][(frame + 1) % 60];

            moveForward = current.forwardVelocity > ForwardAxisThreshold;
            moveBack = current.forwardVelocity < -ForwardAxisThreshold;

            Vector3 worldVelocity = (next.position - current.position) / fixedDeltaTime;
            Vector3 localVelocity = Quaternion.Inverse(Quaternion.Euler(0f, current.rotationX, 0f)) * new Vector3(worldVelocity.x, 0f, worldVelocity.z);

            if (hasPreviousVelocity)
            {
                float lateralAccel = (localVelocity.x - previousLocalVelocityX) / fixedDeltaTime;
                moveLeft = lateralAccel < -StrafeAccelThreshold;
                moveRight = lateralAccel > StrafeAccelThreshold;

                if (worldVelocity.y > JumpImpulseThreshold && previousVerticalVelocity <= JumpImpulseThreshold)
                {
                    // One tick, not a fixed duration - short replays run maybe 70-100 ticks
                    // total, so anything longer risks a flash bleeding into the next press.
                    jumpFlashUntil = Replay.replayPlaybackTime + fixedDeltaTime;
                }
            }

            previousLocalVelocityX = localVelocity.x;
            previousVerticalVelocity = worldVelocity.y;
            hasPreviousVelocity = true;

            ScanEvents();
        }

        private static void ScanEvents()
        {
            Replay.ReplaySession replay = Replay.replay;
            if (replay.events == null)
            {
                return;
            }

            // One tick, not a fixed duration - short replays run maybe 70-100 ticks total, so
            // anything longer risks two close presses reading as one continuous flash.
            float flashUntil = Replay.replayPlaybackTime + Time.fixedDeltaTime;

            for (; nextEventIndex < replay.eventCount; nextEventIndex++)
            {
                Replay.ReplayEventRecord record = replay.events[nextEventIndex / 100][nextEventIndex % 100];
                if (record.timestamp > Replay.replayPlaybackTime)
                {
                    break;
                }

                switch (record.type)
                {
                    case (int)ReplayEvent.FIRE1:
                        fire1FlashUntil = flashUntil;
                        break;
                    case (int)ReplayEvent.FIRE2:
                        fire2FlashUntil = flashUntil;
                        break;
                    case (int)ReplayEvent.GRAVITY_POWER:
                        gravityActive = record.data == 1;
                        break;
                }
            }
        }

        internal static void Draw()
        {
            if (!PluginState.ShowInputOverlay || !Replay.playing || Replay.replay == null)
            {
                return;
            }

            if (keyStyle == null)
            {
                keyStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                timerStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13
                };
            }

            const float keySize = 32f;
            const float gap = 4f;
            float centerX = Screen.width / 2f;
            float topY = Screen.height - 150f;
            float col = keySize + gap;

            DrawKey(centerX - keySize / 2f, topY, keySize, "W", moveForward);
            DrawKey(centerX - col - keySize / 2f, topY + col, keySize, "A", moveLeft);
            DrawKey(centerX - keySize / 2f, topY + col, keySize, "S", moveBack);
            DrawKey(centerX + col - keySize / 2f, topY + col, keySize, "D", moveRight);

            float actionY = topY + col * 2f + 10f;
            DrawKey(centerX - col * 1.5f - keySize / 2f, actionY, keySize, "JMP", Replay.replayPlaybackTime < jumpFlashUntil);
            DrawKey(centerX - col * 0.5f - keySize / 2f, actionY, keySize, "F1", Replay.replayPlaybackTime < fire1FlashUntil);
            DrawKey(centerX + col * 0.5f - keySize / 2f, actionY, keySize, "F2", Replay.replayPlaybackTime < fire2FlashUntil);
            DrawKey(centerX + col * 1.5f - keySize / 2f, actionY, keySize, "GF", gravityActive);

            float fixedDeltaTime = Time.fixedDeltaTime;
            int frame = fixedDeltaTime > 0f
                ? Mathf.Clamp((int)(Replay.replayPlaybackTime / fixedDeltaTime), 0, Mathf.Max(0, Replay.replay.frameCount - 1))
                : 0;
            string timerText = FormatTime(Replay.replayPlaybackTime) + "  |  frame " + frame + "/" + Replay.replay.frameCount;
            GUI.Label(new Rect(centerX - 150f, actionY + keySize + 6f, 300f, 20f), timerText, timerStyle);
        }

        private static void DrawKey(float x, float y, float size, string label, bool active)
        {
            GUI.color = active ? new Color(1f, 0.85f, 0.1f, 0.95f) : new Color(1f, 1f, 1f, 0.25f);
            GUI.Box(new Rect(x, y, size, size), label, keyStyle);
            GUI.color = Color.white;
        }

        private static string FormatTime(float seconds)
        {
            if (seconds < 0f)
            {
                seconds = 0f;
            }

            int minutes = (int)(seconds / 60f);
            float remainder = seconds - minutes * 60f;
            return minutes.ToString("00") + ":" + remainder.ToString("00.000");
        }
    }
}
