using System.Collections.Generic;
using HarmonyLib;
using Steamworks;

namespace SeumSteamOpt
{
    /// <summary>
    /// Workshop.performUpdate polls GetItemState once a second for every subscribed map, and calls
    /// DownloadItem again for each one still flagged as needing an update. The state behind that
    /// flag only moves when Steam raises an install, update or subscription callback, and those are
    /// wired up here to clear the cache, so the poll can be answered locally in between.
    /// </summary>
    internal static class WorkshopPatches
    {
        private struct StateEntry
        {
            internal uint State;
            internal float Time;
        }

        private static readonly Dictionary<ulong, StateEntry> States = new Dictionary<ulong, StateEntry>(64);

        internal static void Apply(Harmony harmony)
        {
            if (SteamOptConfig.ItemStateCacheSeconds.Value <= 0f)
            {
                return;
            }

            Patcher.Patch(harmony, typeof(WorkshopPatches), typeof(SteamUGC), "GetItemState",
                new[] { typeof(PublishedFileId_t) },
                prefix: nameof(GetItemStatePrefix),
                postfix: nameof(GetItemStatePostfix));
        }

        /// <summary>Called from the install / update / subscription callbacks.</summary>
        internal static void Forget(ulong publishedFileId)
        {
            lock (States)
            {
                States.Remove(publishedFileId);
            }
        }

        private static bool GetItemStatePrefix(PublishedFileId_t nPublishedFileID, ref uint __result,
            out bool __state)
        {
            // Harmony runs the postfix even when this prefix skips the original, so it has to know
            // whether __result is a fresh Steam answer or the cached one - otherwise every hit would
            // refresh its own timestamp and the entry would never expire.
            __state = true;

            StateEntry entry;
            lock (States)
            {
                if (!States.TryGetValue(nPublishedFileID.m_PublishedFileId, out entry))
                {
                    return true;
                }
            }

            if (Clock.Now - entry.Time >= SteamOptConfig.ItemStateCacheSeconds.Value)
            {
                return true;
            }

            Counters.Add(ref Counters.ItemStates, 1);
            __result = entry.State;
            __state = false;
            return false;
        }

        private static void GetItemStatePostfix(PublishedFileId_t nPublishedFileID, uint __result,
            bool __state)
        {
            if (!__state)
            {
                return;
            }

            StateEntry entry;
            entry.State = __result;
            entry.Time = Clock.Now;

            lock (States)
            {
                States[nPublishedFileID.m_PublishedFileId] = entry;
            }
        }
    }
}
