using System;
using UnityEngine;

namespace SeumReplay
{
    /// <summary>
    /// The per-frame half of the plugin: prefetching the replays the player is most likely to open,
    /// and keyboard control over playback once one is open.
    ///
    /// It lives on its own DontDestroyOnLoad object rather than on the plugin component so it keeps
    /// running across level loads, the same way the rest of the replay state does.
    /// </summary>
    internal class ReplayRuntime : MonoBehaviour
    {
        private const float MinSpeed = 0.05f;

        // The stock speed slider tops out at 4x; going past it desynchronises playback from the
        // fixed-step events without showing anything useful.
        private const float MaxSpeed = 4f;

        private FPSInputController controller;
        private float resumeSpeed = 1f;

        private int prefetchedLevel = int.MinValue;
        private int prefetchedMutator = int.MinValue;
        private float prefetchedResponse = float.NaN;

        internal static void Create()
        {
            GameObject host = new GameObject("SeumReplayRuntime");
            host.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(host);
            host.AddComponent<ReplayRuntime>();
        }

        private void Update()
        {
            if (!GameRefs.Available)
            {
                return;
            }

            try
            {
                Prefetch();
                Hotkeys();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SeumReplay runtime threw: " + e);
            }
        }

        /// <summary>
        /// Starts the download for the top runs while the player is still on the aim screen, so
        /// that clicking one opens instantly instead of showing a loading box. Fires once per
        /// leaderboard response, and only where the game would actually let a replay be opened.
        /// </summary>
        private void Prefetch()
        {
            int count = ReplayConfig.PrefetchCount.Value;
            if (count <= 0 || !GameRefs.IsState(GameManager.GameplayState.START_LEVEL_AIM))
            {
                return;
            }

            if (Game.currentLevel < 0 || Game.isSpeedrun() || Game.isEndless()
                || Game.startedFrom != StartedFrom.DEFAULT)
            {
                return;
            }

            LevelData levelData = Game.getCurrentLevelData();
            if (levelData == null || levelData.skullWonTier < 2)
            {
                return;
            }

            int mutator = LevelSelector.currentLevelMutator;
            LeaderboardCollection collection = LeaderboardsBackend.leaderboardForLevel(Game.currentLevel, mutator);
            if (collection == null || collection.leaderboards == null || collection.leaderboards.Length == 0)
            {
                return;
            }

            // Board 0 is the global one, which is also the one the replay screen opens by default.
            Leaderboard board = collection.leaderboards[0];
            if (board == null || board.count <= 0 || board.responseTimestamp < 0f)
            {
                return;
            }

            if (prefetchedLevel == Game.currentLevel && prefetchedMutator == mutator
                && prefetchedResponse.Equals(board.responseTimestamp))
            {
                return;
            }

            prefetchedLevel = Game.currentLevel;
            prefetchedMutator = mutator;
            prefetchedResponse = board.responseTimestamp;

            int wanted = Mathf.Min(count, Mathf.Min(board.count, board.scores.Length));
            for (int i = 0; i < wanted; i++)
            {
                Score score = board.scores[i];
                if (score == null || score.replayUGC == ulong.MaxValue)
                {
                    continue;
                }

                if (score.replaySession != null && score.replaySessionUGC == score.replayUGC)
                {
                    continue;
                }

                LeaderboardsBackend.downloadReplay(score);
            }
        }

        private void Hotkeys()
        {
            if (!ReplayConfig.PlaybackHotkeys.Value || !GameRefs.IsState(GameManager.GameplayState.REPLAY))
            {
                return;
            }

            if (Input.GetKeyDown(ReplayConfig.RestartKey.Value))
            {
                if (Replay.replay != null)
                {
                    Replay.playFromStart();
                }
                else
                {
                    Retry(GameRefs.SelectedReplayScore);
                }

                return;
            }

            if (Replay.replay == null)
            {
                return;
            }

            if (Input.GetKeyDown(ReplayConfig.PauseKey.Value))
            {
                TogglePause();
            }

            if (Input.GetKeyDown(ReplayConfig.SlowerKey.Value))
            {
                SetSpeed(Replay.replaySpeed <= MinSpeed ? MinSpeed : Replay.replaySpeed * 0.5f);
            }

            if (Input.GetKeyDown(ReplayConfig.FasterKey.Value))
            {
                SetSpeed(Replay.replaySpeed < MinSpeed ? MinSpeed : Replay.replaySpeed * 2f);
            }

            if (Input.GetKeyDown(ReplayConfig.StepKey.Value))
            {
                Step();
            }
        }

        private void TogglePause()
        {
            if (Replay.replaySpeed > 0.0001f)
            {
                resumeSpeed = Replay.replaySpeed;
                Replay.replaySpeed = 0f;
            }
            else
            {
                Replay.replaySpeed = resumeSpeed > 0.0001f ? resumeSpeed : 1f;
            }
        }

        private void SetSpeed(float speed)
        {
            Replay.replaySpeed = Mathf.Clamp(speed, MinSpeed, MaxSpeed);
            resumeSpeed = Replay.replaySpeed;
        }

        /// <summary>
        /// Advances playback by one recorded frame while paused. Playback is driven from
        /// <c>FPSInputController.Update</c> with the frame's delta time, and a paused replay has a
        /// zero delta, so stepping is just one extra call with the recording's own step.
        /// </summary>
        private void Step()
        {
            if (!Replay.playing || Replay.replaySpeed > 0.0001f)
            {
                return;
            }

            if (controller == null)
            {
                controller = FindObjectOfType<FPSInputController>();
            }

            if (controller != null)
            {
                Replay.simulateStep(controller, Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Re-issues a download the backend gave up on. Clearing the cached handle is what lets the
        /// request through: the backend skips any score whose decoded session already matches its
        /// UGC handle, and a failed attempt leaves that marker behind.
        /// </summary>
        private static void Retry(Score score)
        {
            if (score == null || score.replayUGC == ulong.MaxValue)
            {
                return;
            }

            score.replaySessionUGC = ulong.MaxValue;
            ReplayStatus.Reset(score.replayUGC);
            LeaderboardsBackend.downloadReplay(score);
            Plugin.Log.LogInfo("Retrying replay download for UGC " + score.replayUGC + ".");
        }
    }
}
