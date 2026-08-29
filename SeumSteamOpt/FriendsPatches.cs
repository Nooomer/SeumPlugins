using System;
using System.Collections.Generic;
using HarmonyLib;
using Steamworks;

namespace SeumSteamOpt
{
    /// <summary>
    /// LeaderboardsSteamBackend.update runs from GameManager, LevelSelector and SpeedrunSelector on
    /// every frame. For each finished download it walks all 15 rows and calls both
    /// GetFriendPersonaName and RequestUserInformation on every one of them, and it keeps doing that
    /// until Steam has resolved every player on the board. Refreshing a level-selector zone puts 33
    /// boards in flight at once - up to 495 rows, so close to a thousand marshalled Steam calls per
    /// frame for as long as the names take to arrive.
    ///
    /// Neither answer can change between frames: a persona name only changes when Steam says it did
    /// (PersonaStateChange_t), and RequestUserInformation only flips to false once the fetch it
    /// started has completed.
    /// </summary>
    internal static class FriendsPatches
    {
        private struct NameEntry
        {
            internal string Name;
            internal float Time;
        }

        private struct RequestEntry
        {
            internal bool NameOnly;
            internal bool Result;
            internal float Time;
        }

        private static readonly Dictionary<ulong, NameEntry> Names = new Dictionary<ulong, NameEntry>(256);
        private static readonly Dictionary<ulong, RequestEntry> Requests = new Dictionary<ulong, RequestEntry>(256);

        internal static void Apply(Harmony harmony)
        {
            Type self = typeof(FriendsPatches);

            if (SteamOptConfig.CachePersonaNames.Value)
            {
                Patcher.Patch(harmony, self, typeof(SteamFriends), "GetFriendPersonaName",
                    new[] { typeof(CSteamID) },
                    prefix: nameof(GetFriendPersonaNamePrefix),
                    postfix: nameof(GetFriendPersonaNamePostfix));
            }

            if (SteamOptConfig.UserInfoRequestInterval.Value > 0f)
            {
                Patcher.Patch(harmony, self, typeof(SteamFriends), "RequestUserInformation",
                    new[] { typeof(CSteamID), typeof(bool) },
                    prefix: nameof(RequestUserInformationPrefix),
                    postfix: nameof(RequestUserInformationPostfix));
            }
        }

        /// <summary>
        /// Called from the PersonaStateChange_t callback. Dropping both entries makes the next frame
        /// go back to Steam once, which is exactly the moment the name is worth re-reading.
        /// </summary>
        internal static void Forget(ulong steamId)
        {
            lock (Names)
            {
                Names.Remove(steamId);
            }

            lock (Requests)
            {
                Requests.Remove(steamId);
            }
        }

        // ---------------------------------------------------------------------- persona names

        /// <summary>
        /// A name that came back non-empty is kept until Steam says it changed. An empty one means
        /// Steam has not fetched the user yet, so it is only held for the retry interval - otherwise
        /// a row whose PersonaStateChange never arrives would stay blank forever.
        /// </summary>
        private static bool GetFriendPersonaNamePrefix(CSteamID steamIDFriend, ref string __result,
            out bool __state)
        {
            // Harmony runs postfixes even when a prefix skips the original, so the postfix has to be
            // told whether it is looking at a real Steam answer or at the one we just handed back.
            // Without this the cache would keep re-stamping itself and never expire.
            __state = true;

            NameEntry entry;
            lock (Names)
            {
                if (!Names.TryGetValue(steamIDFriend.m_SteamID, out entry))
                {
                    return true;
                }
            }

            if (string.IsNullOrEmpty(entry.Name)
                && Clock.Now - entry.Time >= Math.Max(SteamOptConfig.UserInfoRequestInterval.Value, 0.25f))
            {
                return true;
            }

            Counters.Add(ref Counters.PersonaNames, 1);
            __result = entry.Name;
            __state = false;
            return false;
        }

        private static void GetFriendPersonaNamePostfix(CSteamID steamIDFriend, string __result,
            bool __state)
        {
            if (!__state)
            {
                return;
            }

            NameEntry entry;
            entry.Name = __result;
            entry.Time = Clock.Now;

            lock (Names)
            {
                Names[steamIDFriend.m_SteamID] = entry;
            }
        }

        // ------------------------------------------------------------------ user info requests

        private static bool RequestUserInformationPrefix(CSteamID steamIDUser, bool bRequireNameOnly,
            ref bool __result, out bool __state)
        {
            __state = true;

            float interval = SteamOptConfig.UserInfoRequestInterval.Value;
            if (interval <= 0f)
            {
                return true;
            }

            RequestEntry entry;
            lock (Requests)
            {
                if (!Requests.TryGetValue(steamIDUser.m_SteamID, out entry))
                {
                    return true;
                }
            }

            // A name-only request and a full request are different questions; only reuse a matching one.
            if (entry.NameOnly != bRequireNameOnly || Clock.Now - entry.Time >= interval)
            {
                return true;
            }

            Counters.Add(ref Counters.UserInfoRequests, 1);
            __result = entry.Result;
            __state = false;
            return false;
        }

        private static void RequestUserInformationPostfix(CSteamID steamIDUser, bool bRequireNameOnly,
            bool __result, bool __state)
        {
            if (!__state)
            {
                return;
            }

            RequestEntry entry;
            entry.NameOnly = bRequireNameOnly;
            entry.Result = __result;
            entry.Time = Clock.Now;

            lock (Requests)
            {
                Requests[steamIDUser.m_SteamID] = entry;
            }
        }
    }
}
