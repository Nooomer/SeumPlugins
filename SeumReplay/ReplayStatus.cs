using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SeumReplay
{
    /// <summary>
    /// What the replay screen currently knows about the replay it is waiting for.
    ///
    /// The stock UI has exactly two states, "session is null" and "session is not null", and shows
    /// "Loading..." for the first one whether the download is in flight, has been retried four
    /// times, or was abandoned twenty seconds ago. This class keeps the missing middle: it reads
    /// the backend's own in-flight request list, asks Steam how far along the transfer is, and
    /// remembers the last hard failure so the screen can say so instead of lying.
    /// </summary>
    internal static class ReplayStatus
    {
        internal enum Phase
        {
            None,
            Downloading,
            Failed,
            Ready,
        }

        private static ulong failedUgc = ulong.MaxValue;
        private static string failureReason;

        // The backend drops a request that ran out of retries without telling anyone, so remember
        // what we last saw in flight: a handle that was being downloaded, is no longer in the list
        // and still has no session, was quietly given up on.
        private static ulong lastInFlightUgc = ulong.MaxValue;

        private static FieldInfo downloadRequestsField;
        private static bool downloadRequestsFieldResolved;

        internal static void ReportFailure(ulong ugcHandle, string reason)
        {
            failedUgc = ugcHandle;
            failureReason = reason;
            Plugin.Log.LogWarning("Replay " + ugcHandle + " failed to load: " + reason);
        }

        internal static void ReportSuccess(ulong ugcHandle)
        {
            if (failedUgc == ugcHandle)
            {
                failedUgc = ulong.MaxValue;
                failureReason = null;
            }

            if (lastInFlightUgc == ugcHandle)
            {
                lastInFlightUgc = ulong.MaxValue;
            }
        }

        /// <summary>Called when a retry is issued by hand, so the screen goes back to waiting.</summary>
        internal static void Reset(ulong ugcHandle)
        {
            ReportSuccess(ugcHandle);
        }

        /// <summary>
        /// Describes the load state of <paramref name="score"/>; <paramref name="detail"/> gets the
        /// line to put under the loading message, or null when there is nothing to add.
        /// </summary>
        internal static Phase Describe(Score score, out string detail)
        {
            detail = null;
            if (score == null)
            {
                return Phase.None;
            }

            if (score.replaySession != null)
            {
                return Phase.Ready;
            }

            if (score.replayUGC == ulong.MaxValue)
            {
                detail = "This run has no replay attached";
                return Phase.Failed;
            }

            int attempt;
            float elapsed;
            if (TryFindInFlight(score.replayUGC, out attempt, out elapsed))
            {
                lastInFlightUgc = score.replayUGC;
                detail = "Downloading" + Progress(score.replayUGC) + FormatElapsed(elapsed)
                    + (attempt > 0 ? "  -  attempt " + (attempt + 1) : "");
                return Phase.Downloading;
            }

            if (failedUgc == score.replayUGC || lastInFlightUgc == score.replayUGC)
            {
                detail = (failedUgc == score.replayUGC && failureReason != null
                        ? failureReason
                        : "Steam did not deliver the replay")
                    + "  -  press " + ReplayConfig.RestartKey.Value + " to retry";
                return Phase.Failed;
            }

            return Phase.None;
        }

        private static string Progress(ulong ugcHandle)
        {
            try
            {
                int downloaded;
                int expected;
                if (Steamworks.SteamRemoteStorage.GetUGCDownloadProgress(new Steamworks.UGCHandle_t(ugcHandle), out downloaded, out expected)
                    && expected > 0)
                {
                    return " " + Mathf.Clamp(Mathf.RoundToInt(100f * downloaded / expected), 0, 100) + "%";
                }
            }
            catch (Exception)
            {
                // Steam is not obliged to know about a handle it has not started on yet.
            }

            return "";
        }

        private static string FormatElapsed(float elapsed)
        {
            return elapsed <= 0f ? "" : "  -  " + elapsed.ToString("0.0") + "s";
        }

        /// <summary>
        /// Looks the UGC handle up in <c>LeaderboardsSteamBackend.downloadReplayRequests</c>, the
        /// private list the backend drives its timeout and retry loop from.
        /// </summary>
        private static bool TryFindInFlight(ulong ugcHandle, out int retries, out float elapsed)
        {
            retries = 0;
            elapsed = 0f;

            if (!downloadRequestsFieldResolved)
            {
                downloadRequestsFieldResolved = true;
                downloadRequestsField = AccessTools.Field(typeof(LeaderboardsSteamBackend), "downloadReplayRequests");
                if (downloadRequestsField == null)
                {
                    Plugin.Log.LogWarning("Could not find LeaderboardsSteamBackend.downloadReplayRequests; "
                        + "the replay screen will not show download progress.");
                }
            }

            if (downloadRequestsField == null)
            {
                return false;
            }

            List<DownloadReplayRequest> requests = downloadRequestsField.GetValue(null) as List<DownloadReplayRequest>;
            if (requests == null)
            {
                return false;
            }

            for (int i = 0; i < requests.Count; i++)
            {
                DownloadReplayRequest request = requests[i];
                if (request == null || request.finished || request.score == null || request.score.replayUGC != ugcHandle)
                {
                    continue;
                }

                retries = request.retries;
                elapsed = Time.unscaledTime - request.timestamp;
                return true;
            }

            return false;
        }
    }
}
