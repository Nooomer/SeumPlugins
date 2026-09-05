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
    /// The two calls are not symmetric, and that difference drove a real bug (fixed in 1.1.4).
    /// GetFriendPersonaName's return value is purely descriptive - the game only displays it - so
    /// caching a resolved name until PersonaStateChange_t fires is safe. RequestUserInformation's
    /// return value is not descriptive: LeaderboardsSteamBackend.update uses it as the game's own
    /// continuation condition, dropping a leaderboard's rows from future polling forever the moment
    /// every row's call returns false in the same frame. Caching either call's answer for any
    /// non-negative interval while a row is still unresolved can hand back a stale "not yet" a moment
    /// after Steam actually resolved it, which - because nothing ever polls that row again - freezes
    /// it on "[unknown]" permanently instead of the few hundred milliseconds vanilla would have taken.
    /// RequestUserInformation is therefore not cached by default at all, and GetFriendPersonaName only
    /// ever caches an answer once it stops being a placeholder.
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
        /// Valve documents "[unknown]" as GetFriendPersonaName's answer for a user Steam has not
        /// fetched yet - it is not a name, it is a pending marker, exactly like an empty string would
        /// be from a saner API. Missing this case was a real bug: caching "[unknown]" as if it were a
        /// resolved name meant it never got re-checked for anyone who is not an actual Steam friend,
        /// because PersonaStateChange_t - the only thing that invalidates this cache - fires for
        /// friends, not for strangers seen on a leaderboard. The result was every non-friend's name
        /// staying "[unknown]" forever instead of resolving once Steam's async fetch completed.
        /// </summary>
        private static bool IsPending(string name)
        {
            return string.IsNullOrEmpty(name) || name == "[unknown]";
        }

        /// <summary>
        /// A resolved name is kept until Steam says it changed - safe, because nothing in the game
        /// reads GetFriendPersonaName's return value to decide anything, only to display it.
        ///
        /// A pending answer is never held back, not even briefly. LeaderboardsSteamBackend.update
        /// stops polling a leaderboard's rows entirely once every row's RequestUserInformation call
        /// returns false in the same frame - so if this prefix ever hands back a stale "still pending"
        /// placeholder in the same frame real Steam data already resolved, that row freezes on the
        /// placeholder forever, because nothing ever asks again. An earlier version held pending
        /// answers for a short interval and that was exactly this bug. Passing every pending call
        /// straight through matches vanilla's own "ask until resolved" behaviour exactly, with none of
        /// the caching's risk - it costs nothing extra either, since vanilla already asks every frame
        /// for a row that has not resolved yet.
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

            if (IsPending(entry.Name))
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

        /// <summary>
        /// Off (interval 0) by default - see the class doc. Caching this call's return value is only
        /// safe once it has settled to false and the caller does not need it to become true again,
        /// which this plugin has no way to know from outside LeaderboardsSteamBackend.update. Enable
        /// only if you have measured this specific loop as a real cost and accept that it can delay a
        /// row's leaderboard entry noticing it is resolved by up to the configured interval - which,
        /// once the row's SteamDownloadRequest has already been dropped from polling, is permanent.
        /// </summary>
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
