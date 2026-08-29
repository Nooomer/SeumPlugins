using HarmonyLib;

namespace SeumFreeCam
{
    /// <summary>
    /// All hooks are about ordering and about keeping the runner's own controls out of the way
    /// while the camera is detached. None of them change what the game does otherwise.
    ///
    /// Each target gets its own patch class with the target on the class itself: a single class
    /// holding several differently-targeted patch methods is the one Harmony arrangement whose
    /// resolution is not obvious from reading it.
    /// </summary>
    internal static class Patches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(CameraPatch));
            harmony.PatchAll(typeof(CursorPatch));
            harmony.PatchAll(typeof(RestartPatch));
            harmony.PatchAll(typeof(CloseReplayPatch));
            harmony.PatchAll(typeof(OpenReplayPatch));
        }

        /// <summary>
        /// The camera's world transform is written in exactly two places, both owned by
        /// <see cref="FPSInputController"/>: <c>tiltCamera</c> sets the camera's local rotation
        /// during <c>Update</c>, and <c>LateUpdate</c> drags the whole view object to the
        /// interpolated head position. Overwriting it in a postfix on that <c>LateUpdate</c> is
        /// therefore the last word on where the camera is, and needs no transpiler and no
        /// reparenting.
        /// </summary>
        [HarmonyPatch(typeof(FPSInputController), "LateUpdate")]
        private static class CameraPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                FreeCam.ApplyToCamera();
            }
        }

        /// <summary>
        /// <c>GameManager.Update</c> recomputes <c>GameCursor.show</c> from the gameplay state every
        /// frame, and a replay counts as a state that wants a cursor, for the leaderboard behind the
        /// playback. While flying we want the opposite: no drawn cursor, and a locked one, so that
        /// mouse movement is look input instead of drifting into the score list. The game's own
        /// <c>syncCursorLock</c> already derives the lock from <c>show</c>, so setting the flag and
        /// calling it is all this takes.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "Update")]
        private static class CursorPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                if (!FreeCam.Active || Hud.showMenu)
                {
                    return;
                }

                GameCursor.show = false;
                GameCursor.syncCursorLock();
            }
        }

        /// <summary>
        /// The game keeps listening for its Restart action during a replay, and acting on it means
        /// tearing the replay down mid-flight: the state only changes on the next frame, the level
        /// reload takes longer still, and the camera you were flying ends up hanging over a level
        /// that is no longer showing a run.
        ///
        /// So while the free camera is on, restart is simply refused. Leave the free camera first
        /// and the key works exactly as it always did. The escape menu is exempt: a restart clicked
        /// there is deliberate, and the free camera does not read input while that menu is open
        /// anyway.
        /// </summary>
        [HarmonyPatch(typeof(Hud), "restartGame")]
        private static class RestartPatch
        {
            [HarmonyPrefix]
            private static bool Prefix()
            {
                return !FreeCam.Active || Hud.showMenu;
            }
        }

        /// <summary>
        /// Closing the replay viewer hands the camera back on the spot rather than a frame or more
        /// later, when the gameplay state finally catches up.
        /// </summary>
        [HarmonyPatch(typeof(Hud), "closeReplay")]
        private static class CloseReplayPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                FreeCam.Exit();
            }
        }

        /// <summary>
        /// Same on the way in, for a replay opened from the win screen while the free camera was
        /// already flying over your own finished run.
        /// </summary>
        [HarmonyPatch(typeof(Hud), "openReplay")]
        private static class OpenReplayPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                FreeCam.Exit();
            }
        }
    }
}
