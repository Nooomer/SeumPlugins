using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Steamworks;

namespace SeumSteamOpt
{
    /// <summary>
    /// Two separate things, both in Workshop.
    ///
    /// Steady state: Workshop.performUpdate polls GetItemState once a second for every subscribed
    /// map, and calls DownloadItem again for each one still flagged as needing an update. The state
    /// behind that flag only moves when Steam raises an install, update or subscription callback,
    /// and those are wired up here to clear the cache, so the poll can be answered locally between.
    ///
    /// Startup: MainMenu.Start calls Workshop.init, which loads the entire workshop library before
    /// anyone has asked to see it. refreshAllDownloadedMaps queries UGC details for every subscribed
    /// map, and the callback runs fillMapInfo per map, which starts an HTTP download of that map's
    /// preview image and fires a GetUserItemVote and a GetAppDependencies call. checkIfUserMapsArePublished
    /// then sends a further UGC query on every return to the main menu. All of it feeds
    /// subscribedMapInfos and userMaps, which nothing outside Workshop.cs reads and only
    /// Workshop.draw displays - and Workshop.draw only runs once the player opens the screen.
    ///
    /// Background, for the life of the session: Workshop.init registers a PersonaStateChange_t
    /// callback in its "if (firstInit)" block, and that registration is never gated by whether the
    /// screen is open. onPersonaChanged fires for every status change of every friend - going online,
    /// changing game, anything - and unconditionally calls GetLargeFriendAvatar, then GetImageSize and
    /// GetImageRGBA to decode a full 184x184 RGBA image (135 KB), before even checking whether that
    /// friend authored anything in subscribedMapInfos or userMaps. For the overwhelming majority of
    /// friends, on the overwhelming majority of status changes, that decode is thrown away unused.
    /// </summary>
    internal static class WorkshopPatches
    {
        private struct StateEntry
        {
            internal uint State;
            internal float Time;
        }

        private static readonly Dictionary<ulong, StateEntry> States = new Dictionary<ulong, StateEntry>(64);

        /// <summary>
        /// Set the moment the workshop screen first updates. Also the re-entrancy gate: the deferred
        /// calls are made after this flips, so their own prefixes wave them through.
        /// </summary>
        private static bool workshopOpened;

        private static MethodInfo refreshAllDownloadedMaps;
        private static MethodInfo refreshUserMaps;
        private static MethodInfo checkIfUserMapsArePublished;
        private static FieldInfo userMapsField;

        private static FieldInfo subscribedMapInfosField;
        private static FieldInfo mapInfoAuthorIdField;

        internal static void Apply(Harmony harmony)
        {
            Type self = typeof(WorkshopPatches);

            if (SteamOptConfig.ItemStateCacheSeconds.Value > 0f)
            {
                Patcher.Patch(harmony, self, typeof(SteamUGC), "GetItemState",
                    new[] { typeof(PublishedFileId_t) },
                    prefix: nameof(GetItemStatePrefix),
                    postfix: nameof(GetItemStatePostfix));
            }

            refreshAllDownloadedMaps = AccessTools.Method(typeof(Workshop), "refreshAllDownloadedMaps");
            refreshUserMaps = AccessTools.Method(typeof(Workshop), "refreshUserMaps");
            checkIfUserMapsArePublished = AccessTools.Method(typeof(Workshop), "checkIfUserMapsArePublished");
            userMapsField = AccessTools.Field(typeof(Workshop), "userMaps");

            if (SteamOptConfig.DeferWorkshopStartup.Value
                && refreshAllDownloadedMaps != null
                && refreshUserMaps != null
                && checkIfUserMapsArePublished != null)
            {
                // Workshop.update only runs while the screen is active, so it is the exact moment the
                // deferred work becomes worth doing.
                Patcher.Patch(harmony, self, typeof(Workshop), "update",
                    prefix: nameof(WorkshopUpdatePrefix));

                Patcher.Patch(harmony, self, typeof(Workshop), "refreshAllDownloadedMaps",
                    prefix: nameof(RefreshAllDownloadedMapsPrefix));

                // Unlike refreshAllDownloadedMaps, this one is not gated by Workshop's own firstInit
                // flag - it reruns unconditionally on every single MainMenu.Start, i.e. every visit to
                // the main menu, and for each locally authored map on disk it re-reads
                // SteamUser.GetSteamID() and SteamFriends.GetPersonaName() - identity that cannot
                // change and that CacheSteamIdentity does not reach, because this call site is
                // separate from SeumSteam.init.
                Patcher.Patch(harmony, self, typeof(Workshop), "refreshUserMaps",
                    prefix: nameof(RefreshUserMapsPrefix));
            }

            if (SteamOptConfig.DeferWorkshopStartup.Value || SteamOptConfig.SkipEmptyUserMapQuery.Value)
            {
                Patcher.Patch(harmony, self, typeof(Workshop), "checkIfUserMapsArePublished",
                    prefix: nameof(CheckIfUserMapsArePublishedPrefix));
            }

            if (SteamOptConfig.SkipAvatarsForUnknownAuthors.Value)
            {
                subscribedMapInfosField = AccessTools.Field(typeof(Workshop), "subscribedMapInfos");
                Type mapInfoType = AccessTools.Inner(typeof(Workshop), "MapInfo");
                mapInfoAuthorIdField = mapInfoType == null ? null : AccessTools.Field(mapInfoType, "authorId");

                if (subscribedMapInfosField == null || userMapsField == null || mapInfoAuthorIdField == null)
                {
                    Plugin.Log.LogWarning("Workshop.subscribedMapInfos/userMaps/MapInfo.authorId not "
                        + "found; avatar filtering for background persona changes disabled.");
                }
                else
                {
                    Patcher.Patch(harmony, self, typeof(Workshop), "onPersonaChanged",
                        prefix: nameof(OnPersonaChangedPrefix));
                }
            }
        }

        // ------------------------------------------------------------------- deferred startup

        /// <summary>
        /// Void prefix: the original always runs. This only notes that the screen is now open and
        /// pays the load that was held back.
        /// </summary>
        private static void WorkshopUpdatePrefix()
        {
            if (workshopOpened)
            {
                return;
            }

            workshopOpened = true;

            try
            {
                refreshAllDownloadedMaps.Invoke(null, null);
                refreshUserMaps.Invoke(null, null);
                checkIfUserMapsArePublished.Invoke(null, null);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("deferred workshop load failed: " + e.Message);
            }
        }

        private static bool RefreshAllDownloadedMapsPrefix()
        {
            if (workshopOpened || !SteamOptConfig.DeferWorkshopStartup.Value)
            {
                return true;
            }

            Counters.Add(ref Counters.WorkshopQueries, 1);
            return false;
        }

        /// <summary>
        /// Unlike the other two deferred calls, this one is not asynchronous - it does no Steam
        /// calls of its own on a healthy vanilla run, only a local disk scan and, per locally authored
        /// map, SteamUser.GetSteamID()/SteamFriends.GetPersonaName(). Counted separately from
        /// WorkshopQueries would be more precise, but it shares the same "held back until the screen
        /// opens" story, so it is folded into the same counter rather than adding a third one.
        /// </summary>
        private static bool RefreshUserMapsPrefix()
        {
            if (workshopOpened || !SteamOptConfig.DeferWorkshopStartup.Value)
            {
                return true;
            }

            Counters.Add(ref Counters.WorkshopQueries, 1);
            return false;
        }

        private static bool CheckIfUserMapsArePublishedPrefix()
        {
            if (SteamOptConfig.DeferWorkshopStartup.Value && !workshopOpened)
            {
                Counters.Add(ref Counters.WorkshopQueries, 1);
                return false;
            }

            if (SteamOptConfig.SkipEmptyUserMapQuery.Value && !HasUserMaps())
            {
                Counters.Add(ref Counters.WorkshopQueries, 1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// True only when there is at least one locally authored map to ask Steam about. MapInfo is a
        /// private nested type, so the list is read as a plain collection - the count is all that is
        /// needed to know whether the query would carry any ids at all.
        /// </summary>
        private static bool HasUserMaps()
        {
            if (userMapsField == null)
            {
                // Unknown means "behave like vanilla".
                return true;
            }

            try
            {
                ICollection maps = userMapsField.GetValue(null) as ICollection;
                return maps == null || maps.Count > 0;
            }
            catch (Exception)
            {
                return true;
            }
        }

        // ------------------------------------------------------------ background avatar filtering

        /// <summary>
        /// Skips the avatar fetch entirely for a persona change from someone who has not authored
        /// anything the player has subscribed to or created - the only two places
        /// setAvatarImage's result can end up. Letting the original run for an actual author is
        /// required: that is the whole point of the callback existing.
        /// </summary>
        private static bool OnPersonaChangedPrefix(PersonaStateChange_t info)
        {
            if (IsKnownAuthor(info.m_ulSteamID))
            {
                return true;
            }

            Counters.Add(ref Counters.WorkshopQueries, 1);
            return false;
        }

        private static bool IsKnownAuthor(ulong steamId)
        {
            try
            {
                return ListHasAuthor(subscribedMapInfosField.GetValue(null) as IEnumerable, steamId)
                    || ListHasAuthor(userMapsField.GetValue(null) as IEnumerable, steamId);
            }
            catch (Exception)
            {
                // Not knowing means "let it through" - the original behaviour, just uncached.
                return true;
            }
        }

        private static bool ListHasAuthor(IEnumerable maps, ulong steamId)
        {
            if (maps == null)
            {
                return false;
            }

            foreach (object map in maps)
            {
                if (map != null && (ulong)mapInfoAuthorIdField.GetValue(map) == steamId)
                {
                    return true;
                }
            }

            return false;
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
