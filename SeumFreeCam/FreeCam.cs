using UnityEngine;

namespace SeumFreeCam
{
    /// <summary>
    /// The camera state itself, kept apart from the code that feeds it so that input (which runs in
    /// <c>Update</c>) and the transform write (which has to run after the game's own
    /// <c>LateUpdate</c>) can never disagree about where the camera is.
    ///
    /// Nothing here reparents or clones anything: the game's own <c>playerCamera</c> stays the
    /// camera that renders, and we only overwrite its world transform once the game is done writing
    /// its own. That is what keeps SMAA, SSAO, motion blur and the fog exactly as they were.
    /// </summary>
    internal static class FreeCam
    {
        internal static bool Active;

        private static Vector3 position;
        private static float yaw;
        private static float pitch;

        /// <summary>Weapon camera we blanked, so we can hand it back its own culling mask.</summary>
        private static Camera maskedWeaponCamera;
        private static int weaponCameraMask;

        // The camera's local offset from the view object, as the prefab set it. Nothing in the game
        // ever writes it again — LateUpdate moves the parent and tiltCamera writes only a local
        // rotation — so the world position we push in gets baked into that offset and stays there
        // after we stop, leaving the camera stranded metres away from the body. Hence putting it
        // back by hand.
        private static Transform restoreTarget;
        private static Vector3 restoreLocalPosition;
        private static Quaternion restoreLocalRotation;

        internal static Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        internal static Quaternion Rotation
        {
            get { return Quaternion.Euler(pitch, yaw, 0f); }
        }

        internal static Vector3 Forward
        {
            get { return Rotation * Vector3.forward; }
        }

        internal static Vector3 Right
        {
            get { return Rotation * Vector3.right; }
        }

        /// <summary>
        /// The camera that renders the game. Resolved every time rather than cached: the character
        /// prefab is destroyed and re-instantiated on every level load, and a replay restarting its
        /// loop goes through <c>Game.restartLevel</c>.
        /// </summary>
        internal static Camera PlayerCamera
        {
            get
            {
                CharacterView view = CharacterView.instance;
                if (view == null || view.playerCamera == null)
                {
                    return null;
                }

                return view.playerCamera.GetComponent<Camera>();
            }
        }

        private static Camera WeaponCamera
        {
            get
            {
                CharacterView view = CharacterView.instance;
                if (view == null || view.weaponCamera == null)
                {
                    return null;
                }

                return view.weaponCamera.GetComponent<Camera>();
            }
        }

        /// <summary>
        /// Takes over from wherever the runner is looking right now, so the switch is a seamless
        /// detach rather than a jump.
        /// </summary>
        internal static void Enter()
        {
            Camera camera = PlayerCamera;
            if (camera == null)
            {
                return;
            }

            RememberLocalTransform(camera.transform);

            position = camera.transform.position;
            Vector3 euler = camera.transform.rotation.eulerAngles;
            yaw = euler.y;
            pitch = NormalizeAngle(euler.x);
            Active = true;
        }

        internal static void Exit()
        {
            Active = false;
            RestoreLocalTransform();
            RestoreHands();
            PlayerMarker.Hide();
        }

        internal static void AddLook(float yawDelta, float pitchDelta)
        {
            yaw += yawDelta;
            pitch = Mathf.Clamp(pitch + pitchDelta, -89f, 89f);
        }

        /// <summary>Points the camera at a world position without moving it.</summary>
        internal static void LookAt(Vector3 target)
        {
            Vector3 direction = target - position;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 euler = Quaternion.LookRotation(direction).eulerAngles;
            yaw = euler.y;
            pitch = Mathf.Clamp(NormalizeAngle(euler.x), -89f, 89f);
        }

        /// <summary>
        /// Called from the postfix on <c>FPSInputController.LateUpdate</c>, which is the last thing
        /// in the frame that touches the camera: <c>tiltCamera</c> writes its local rotation from
        /// <c>Update</c>, and <c>LateUpdate</c> moves the whole view object from the interpolation
        /// buffer. Writing after both means we win without having to suppress either.
        /// </summary>
        internal static void ApplyToCamera()
        {
            if (!Active)
            {
                return;
            }

            Camera camera = PlayerCamera;
            if (camera == null)
            {
                return;
            }

            // A replay looping back to its start goes through Game.restartLevel, which can hand us
            // a freshly instantiated character and therefore a different camera. Capture its
            // untouched offset before this frame's write, or there is nothing to restore later.
            RememberLocalTransform(camera.transform);

            camera.transform.position = position;
            camera.transform.rotation = Rotation;

            if (FreeCamConfig.LockFov.Value && GameSettings.settings != null)
            {
                camera.fieldOfView = GameSettings.settings.fov;
            }

            if (FreeCamConfig.HideHands.Value)
            {
                HideHands();
            }
        }

        /// <summary>
        /// Blanks the weapon camera's culling mask instead of disabling the camera. The hands are
        /// the only thing it draws, but the ghost and shadow-world full-screen effects hang off it,
        /// and disabling the component would take those with it.
        /// </summary>
        private static void HideHands()
        {
            Camera weapon = WeaponCamera;
            if (weapon == null)
            {
                return;
            }

            if (maskedWeaponCamera != weapon)
            {
                RestoreHands();
                maskedWeaponCamera = weapon;
                weaponCameraMask = weapon.cullingMask;
            }

            weapon.cullingMask = 0;
        }

        private static void RememberLocalTransform(Transform cameraTransform)
        {
            if (restoreTarget == cameraTransform)
            {
                return;
            }

            restoreTarget = cameraTransform;
            restoreLocalPosition = cameraTransform.localPosition;
            restoreLocalRotation = cameraTransform.localRotation;
        }

        private static void RestoreLocalTransform()
        {
            if (restoreTarget != null)
            {
                restoreTarget.localPosition = restoreLocalPosition;
                restoreTarget.localRotation = restoreLocalRotation;
            }

            restoreTarget = null;
        }

        internal static void RestoreHands()
        {
            if (maskedWeaponCamera != null)
            {
                maskedWeaponCamera.cullingMask = weaponCameraMask;
            }

            maskedWeaponCamera = null;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
