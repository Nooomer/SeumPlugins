using System;
using Rewired;
using UnityEngine;

namespace SeumFreeCam
{
    /// <summary>
    /// Reads input and decides where the camera should be. It never writes to the camera itself —
    /// <see cref="Patches"/> does that at the one point in the frame where the write survives.
    ///
    /// Lives on its own DontDestroyOnLoad object because the character, and with it every game
    /// object we care about, is thrown away and rebuilt on each level load.
    /// </summary>
    internal class FreeCamRuntime : MonoBehaviour
    {
        // Rewired action ids, from RewiredConsts.Action. The game itself looks up mouse look with
        // GetAxis(2)/GetAxis(3), so reusing them gives us the player's own bindings, their
        // sensitivity, and gamepad sticks for free.
        private const int MoveHorizontal = 0;
        private const int MoveVertical = 1;
        private const int LookHorizontal = 2;
        private const int LookVertical = 3;

        private const float MinSpeed = 0.5f;
        private const float MaxSpeed = 400f;

        private GameManager gameManager;
        private FPSInputController controller;
        private Player input;
        private float speed;

        internal static void Create()
        {
            GameObject host = new GameObject("SeumFreeCamRuntime");
            host.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(host);
            host.AddComponent<FreeCamRuntime>();
        }

        private void Awake()
        {
            speed = FreeCamConfig.MoveSpeed.Value;
        }

        private void Update()
        {
            try
            {
                Tick();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SeumFreeCam runtime threw, leaving free camera mode: " + e);
                FreeCam.Exit();
            }
        }

        private void OnDestroy()
        {
            FreeCam.Exit();
            PlayerMarker.Destroy();
        }

        private void Tick()
        {
            if (!InReplay())
            {
                if (FreeCam.Active)
                {
                    FreeCam.Exit();
                }

                return;
            }

            if (Input.GetKeyDown(FreeCamConfig.ToggleKey.Value))
            {
                if (FreeCam.Active)
                {
                    FreeCam.Exit();
                }
                else
                {
                    speed = FreeCamConfig.MoveSpeed.Value;
                    FreeCam.Enter();
                }
            }

            if (!FreeCam.Active)
            {
                return;
            }

            PlayerMarker.Follow(Runner());

            // The pause menu takes the mouse back, and an unfocused window has no input worth
            // reading. The camera stays where it is in both cases.
            if (Hud.showMenu || !Game.inFocus)
            {
                return;
            }

            Look();
            Move();

            if (Input.GetKeyDown(FreeCamConfig.SnapKey.Value))
            {
                Snap();
            }
        }

        /// <summary>
        /// True while the game is running a recorded run: the replay viewer opened from a
        /// leaderboard, and optionally your own run playing behind the win screen.
        /// </summary>
        private bool InReplay()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                if (gameManager == null)
                {
                    return false;
                }
            }

            GameManager.GameplayState state = gameManager.gameplayState;
            if (state == GameManager.GameplayState.REPLAY)
            {
                return true;
            }

            return FreeCamConfig.OnWinScreen.Value
                && state == GameManager.GameplayState.FINISH_LEVEL
                && Replay.playing;
        }

        private FPSInputController Runner()
        {
            if (controller == null)
            {
                controller = FindObjectOfType<FPSInputController>();
            }

            return controller;
        }

        private void Look()
        {
            if (!ReInput.isReady)
            {
                return;
            }

            if (input == null)
            {
                input = ReInput.players.GetPlayer(0);
                if (input == null)
                {
                    return;
                }
            }

            float sensitivity = FreeCamConfig.LookSensitivity.Value;
            if (GameSettings.settings != null)
            {
                sensitivity *= GameSettings.settings.mouseSensitivity;
            }

            float yawDelta = input.GetAxis(LookHorizontal) * sensitivity;
            float pitchDelta = input.GetAxis(LookVertical) * sensitivity;

            if (GameSettings.settings != null)
            {
                pitchDelta *= GameSettings.settings.mouseLookModifierY;
            }

            // The game stores pitch as "how far up", then feeds it in as a negative Euler X, so a
            // positive look axis has to lower our Euler pitch to mean the same thing.
            FreeCam.AddLook(yawDelta, FreeCamConfig.InvertY.Value ? pitchDelta : -pitchDelta);
        }

        private void Move()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                speed = Mathf.Clamp(speed * Mathf.Pow(1.2f, scroll), MinSpeed, MaxSpeed);
            }

            Vector3 direction = Vector3.zero;
            if (input != null)
            {
                direction += FreeCam.Right * input.GetAxis(MoveHorizontal);
                direction += FreeCam.Forward * input.GetAxis(MoveVertical);
            }

            if (Input.GetKey(FreeCamConfig.UpKey.Value))
            {
                direction += Vector3.up;
            }

            if (Input.GetKey(FreeCamConfig.DownKey.Value))
            {
                direction += Vector3.down;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float current = speed;
            if (Input.GetKey(FreeCamConfig.FastKey.Value))
            {
                current *= FreeCamConfig.FastMultiplier.Value;
            }

            if (Input.GetKey(FreeCamConfig.SlowKey.Value))
            {
                current *= FreeCamConfig.SlowMultiplier.Value;
            }

            // Unscaled, because pausing or slowing the replay must not slow the camera down with it.
            FreeCam.Position += Vector3.ClampMagnitude(direction, 1f) * current * Time.unscaledDeltaTime;
        }

        /// <summary>
        /// Drops the camera a few metres behind and above the runner and points it at them. The
        /// offset uses the runner's own facing, so you end up looking over their shoulder.
        /// </summary>
        private void Snap()
        {
            FPSInputController runner = Runner();
            if (runner == null)
            {
                return;
            }

            Vector3 target = runner.transform.position;
            FreeCam.Position = target - runner.transform.forward * 5f + Vector3.up * 2f;
            FreeCam.LookAt(target);
        }
    }
}
