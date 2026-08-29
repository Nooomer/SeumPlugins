using System.Reflection;
using HarmonyLib;

namespace SeumReplay
{
    /// <summary>
    /// The two pieces of replay state the game keeps private on <see cref="Hud"/>: which score the
    /// replay screen is showing, and the GameManager it asks for the current gameplay state.
    /// Resolved once and cached, because both are read every frame while a replay is on screen.
    /// </summary>
    internal static class GameRefs
    {
        private static FieldInfo selectedReplayScoreField;
        private static FieldInfo gameManagerField;
        private static bool resolved;

        internal static void Resolve()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            selectedReplayScoreField = AccessTools.Field(typeof(Hud), "selectedReplayScore");
            gameManagerField = AccessTools.Field(typeof(Hud), "gameManager");

            if (selectedReplayScoreField == null || gameManagerField == null)
            {
                Plugin.Log.LogWarning("Could not resolve Hud.selectedReplayScore / Hud.gameManager; "
                    + "the replay status line and playback hotkeys will stay off.");
            }
        }

        internal static bool Available
        {
            get { return selectedReplayScoreField != null && gameManagerField != null; }
        }

        internal static Score SelectedReplayScore
        {
            get
            {
                return selectedReplayScoreField == null ? null : selectedReplayScoreField.GetValue(null) as Score;
            }
        }

        internal static GameManager Manager
        {
            get
            {
                return gameManagerField == null ? null : gameManagerField.GetValue(null) as GameManager;
            }
        }

        internal static bool IsState(GameManager.GameplayState state)
        {
            GameManager manager = Manager;
            return manager != null && manager.gameplayState == state;
        }
    }
}
