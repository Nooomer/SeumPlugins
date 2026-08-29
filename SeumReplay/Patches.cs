using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Steamworks;

namespace SeumReplay
{
    /// <summary>
    /// The read side of the replay pipeline: decode, fetch, and the backend's retry loop.
    ///
    /// Deliberately absent: anything on <c>Replay.record</c>, <c>Replay.packageReplay</c> or
    /// <c>Replay.replaySummary</c>. Those produce the bytes that go up to the Steam leaderboard,
    /// and an unmodded client has to be able to read them back, so the recording side stays exactly
    /// as the game shipped it.
    /// </summary>
    internal static class Patches
    {
        internal static void Apply(Harmony harmony)
        {
            if (ReplayConfig.FixChunkTails.Value)
            {
                Patch(harmony, typeof(UnpackReplayPatch));
            }
            else
            {
                Plugin.Log.LogWarning("FixChunkTails is off - replays whose frame/event/projectile "
                    + "counts divide evenly by 60/100/20 will keep decoding incorrectly.");
            }

            Patch(harmony, typeof(DownloadScoreReplayPatch));
            Patch(harmony, typeof(ReplayDownloadedPatch));
            Patch(harmony, typeof(RequestTimeoutPatch));
            Patch(harmony, typeof(ReplayHud.ReplayUIPatch));
        }

        /// <summary>
        /// One patch class at a time, each in its own try/catch: a batch patch gives up on the
        /// first failure and silently drops everything declared after it, and these patches are
        /// independent - a transpiler that stops matching some future build should not take the
        /// decoder fix down with it.
        /// </summary>
        private static void Patch(Harmony harmony, Type type)
        {
            try
            {
                new PatchClassProcessor(harmony, type).Patch();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Failed to apply Harmony patch '" + type.FullName + "': " + e);
            }
        }

        /// <summary>
        /// Swaps the stock decoder for <see cref="ReplayCodec"/>.
        /// </summary>
        [HarmonyPatch(typeof(Replay), nameof(Replay.unpackReplay))]
        private static class UnpackReplayPatch
        {
            private static bool Prefix(byte[] data, ref Replay.ReplaySession __result)
            {
                try
                {
                    __result = ReplayCodec.Unpack(data);
                }
                catch (Exception e)
                {
                    // The stock decoder is called from a Steam callback that sets its "finished"
                    // flag on the last line, so anything thrown here used to strand the request.
                    Plugin.Log.LogError("Failed to unpack a replay: " + e);
                    __result = null;
                }

                return false;
            }
        }

        /// <summary>
        /// Serves a replay straight from the disk cache when we already have its bytes, so no Steam
        /// round trip happens at all. Falls through to the stock request otherwise.
        /// </summary>
        [HarmonyPatch(typeof(LeaderboardsSteamBackend), nameof(LeaderboardsSteamBackend.downloadScoreReplay))]
        private static class DownloadScoreReplayPatch
        {
            private static bool Prefix(Score score, int retries)
            {
                if (score == null || score.replayUGC == ulong.MaxValue)
                {
                    return true;
                }

                if (score.replaySession != null && score.replaySessionUGC == score.replayUGC)
                {
                    return false;
                }

                // A retry means the last attempt at these bytes did not work out; go to Steam.
                if (retries > 0 || !ReplayConfig.DiskCache.Value)
                {
                    return true;
                }

                byte[] data;
                if (!ReplayCache.TryLoad(score.replayUGC, out data))
                {
                    return true;
                }

                Replay.ReplaySession session = null;
                try
                {
                    session = ReplayCodec.Unpack(data);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogWarning("Cached replay " + score.replayUGC + " did not decode: " + e.Message);
                }

                if (session == null)
                {
                    ReplayCache.Remove(score.replayUGC);
                    return true;
                }

                score.replaySession = session;
                score.replaySessionUGC = score.replayUGC;
                ReplayStatus.ReportSuccess(score.replayUGC);
                return false;
            }
        }

        /// <summary>
        /// Replaces the Steam download callback.
        ///
        /// The stock version sets <c>finished</c> as its final statement, so any exception on the
        /// way there - a short read, a malformed blob - leaves the request pinned in the backend's
        /// list and the UI on "Loading..." with nothing else to say. This one always finishes the
        /// request, reports what went wrong, and puts the raw bytes in the disk cache on the way
        /// past.
        /// </summary>
        [HarmonyPatch(typeof(DownloadReplayRequest), "replayDownloaded")]
        private static class ReplayDownloadedPatch
        {
            private static bool Prefix(DownloadReplayRequest __instance, RemoteStorageDownloadUGCResult_t callback, bool failure)
            {
                // Copied out of the patch parameter once: the Harmony analyzer reads any member
                // access on a by-value struct argument as an attempt to write it back.
                RemoteStorageDownloadUGCResult_t result = callback;
                Score score = __instance.score;
                ulong handle = score != null ? score.replayUGC : result.m_hFile.m_UGCHandle;

                try
                {
                    if (failure)
                    {
                        ReplayStatus.ReportFailure(handle, "Steam call failed");
                        return false;
                    }

                    if (result.m_eResult != EResult.k_EResultOK)
                    {
                        ReplayStatus.ReportFailure(handle, "Steam returned " + result.m_eResult);
                        if (score != null)
                        {
                            score.replaySessionUGC = ulong.MaxValue;
                        }

                        return false;
                    }

                    if (result.m_nSizeInBytes <= 0)
                    {
                        ReplayStatus.ReportFailure(handle, "Steam returned an empty replay");
                        return false;
                    }

                    byte[] data = new byte[result.m_nSizeInBytes];
                    int read = SteamRemoteStorage.UGCRead(result.m_hFile, data, data.Length, 0u,
                        EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);

                    if (read <= 0)
                    {
                        ReplayStatus.ReportFailure(handle, "Steam delivered no bytes");
                        return false;
                    }

                    if (read < data.Length)
                    {
                        Plugin.Log.LogWarning("Replay " + handle + " came back short: " + read + " of "
                            + data.Length + " B.");
                        Array.Resize(ref data, read);
                    }

                    Replay.ReplaySession session = ReplayCodec.Unpack(data);
                    if (session == null)
                    {
                        ReplayStatus.ReportFailure(handle, "Replay data could not be decoded");
                        if (score != null)
                        {
                            score.replaySessionUGC = ulong.MaxValue;
                        }

                        return false;
                    }

                    if (score != null)
                    {
                        score.replaySession = session;
                        score.replaySessionUGC = score.replayUGC;
                    }

                    // Only cache blobs that decoded cleanly and in full, so a bad download does not
                    // become a permanently bad cache entry.
                    if (ReplayConfig.DiskCache.Value && !ReplayCodec.LastUnpackWasTruncated)
                    {
                        ReplayCache.Store(handle, data);
                    }

                    ReplayStatus.ReportSuccess(handle);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Replay download handling threw: " + e);
                    ReplayStatus.ReportFailure(handle, "Replay could not be read");
                }
                finally
                {
                    __instance.finished = true;
                }

                return false;
            }
        }

        /// <summary>
        /// Rewrites the two constants the backend's replay retry loop is built on: the three-second
        /// window a download gets before it is cancelled and re-issued, and the five attempts it
        /// gets in total. Three seconds is short enough that a merely slow connection never
        /// finishes anything - each attempt is killed just before it lands, and after the fifth the
        /// backend stops without a word.
        /// </summary>
        [HarmonyPatch(typeof(LeaderboardsSteamBackend), nameof(LeaderboardsSteamBackend.update))]
        private static class RequestTimeoutPatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                float timeout = ReplayConfig.DownloadTimeout.Value;
                int retries = ReplayConfig.DownloadRetries.Value;
                bool timeoutPatched = false;
                bool retriesPatched = false;

                foreach (CodeInstruction instruction in instructions)
                {
                    if (!timeoutPatched && instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float
                        && Math.Abs((float)instruction.operand - 3f) < 0.0001f)
                    {
                        timeoutPatched = true;
                        yield return new CodeInstruction(OpCodes.Ldc_R4, timeout).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                        continue;
                    }

                    if (!retriesPatched && instruction.opcode == OpCodes.Ldc_I4_5)
                    {
                        retriesPatched = true;
                        yield return new CodeInstruction(OpCodes.Ldc_I4, retries).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                        continue;
                    }

                    yield return instruction;
                }

                if (!timeoutPatched || !retriesPatched)
                {
                    Plugin.Log.LogWarning("LeaderboardsSteamBackend.update did not look the way it was "
                        + "expected to (timeout patched: " + timeoutPatched + ", retries patched: "
                        + retriesPatched + "); the stock 3s/5-attempt limits stay in place.");
                }
            }
        }
    }
}
