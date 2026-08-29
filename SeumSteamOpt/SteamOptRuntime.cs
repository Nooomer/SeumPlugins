using System;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeumSteamOpt
{
    /// <summary>
    /// Owns the two things the patches cannot own themselves: the Steam callbacks that tell the
    /// caches when their answers went stale, and the periodic log line that shows how much traffic
    /// has actually been saved.
    ///
    /// The callbacks can only be registered after SteamAPI.Init has succeeded, which happens in the
    /// game's own SteamManager.Awake, so registration waits for that rather than running at plugin
    /// load. Dispatch itself is free: SteamManager.Update already calls SteamAPI.RunCallbacks every
    /// frame.
    /// </summary>
    internal class SteamOptRuntime : MonoBehaviour
    {
        private static SteamOptRuntime instance;

        // Steamworks.NET unregisters a Callback when it is collected, so these have to stay rooted.
        private static Callback<PersonaStateChange_t> personaChanged;
        private static Callback<ItemInstalled_t> itemInstalled;
        private static Callback<RemoteStoragePublishedFileUpdated_t> itemUpdated;
        private static Callback<RemoteStoragePublishedFileSubscribed_t> itemSubscribed;
        private static Callback<RemoteStoragePublishedFileUnsubscribed_t> itemUnsubscribed;

        private bool callbacksRegistered;
        private float nextLog;

        internal static void Create()
        {
            if (instance != null)
            {
                return;
            }

            GameObject host = new GameObject("SeumSteamOpt");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<SteamOptRuntime>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// A scene load means the player has arrived somewhere new, so the refresh that follows is a
        /// first look rather than a repeat and must not be deduped away. VelocityMeter's leaderboard
        /// range editor depends on this: it applies a new range by reloading the Game scene, and the
        /// range only takes effect on the download that reload triggers.
        /// </summary>
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LeaderboardPatches.OnSceneChanged();
        }

        private void Update()
        {
            if (!callbacksRegistered)
            {
                TryRegisterCallbacks();
            }

            float interval = SteamOptConfig.StatsLogInterval.Value;
            if (interval <= 0f)
            {
                return;
            }

            if (Time.unscaledTime >= nextLog)
            {
                nextLog = Time.unscaledTime + interval;
                Plugin.Log.LogInfo(Counters.Summary());
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                Plugin.Log.LogInfo(Counters.Summary());
                instance = null;
            }
        }

        private void TryRegisterCallbacks()
        {
            if (!SteamState.Initialized)
            {
                return;
            }

            try
            {
                if (SteamOptConfig.CachePersonaNames.Value
                    || SteamOptConfig.UserInfoRequestInterval.Value > 0f)
                {
                    personaChanged = Callback<PersonaStateChange_t>.Create(OnPersonaChanged);
                }

                if (SteamOptConfig.ItemStateCacheSeconds.Value > 0f)
                {
                    itemInstalled = Callback<ItemInstalled_t>.Create(OnItemInstalled);
                    itemUpdated = Callback<RemoteStoragePublishedFileUpdated_t>.Create(OnItemUpdated);
                    itemSubscribed = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnItemSubscribed);
                    itemUnsubscribed = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnItemUnsubscribed);
                }
            }
            catch (Exception e)
            {
                // Without the callbacks the caches still expire on their own timers, so this is a
                // degradation rather than a failure - but it is worth knowing about.
                Plugin.Log.LogWarning("could not register Steam callbacks, caches fall back to their "
                    + "expiry timers: " + e.Message);
            }

            callbacksRegistered = true;
        }

        private static void OnPersonaChanged(PersonaStateChange_t info)
        {
            FriendsPatches.Forget(info.m_ulSteamID);
        }

        private static void OnItemInstalled(ItemInstalled_t info)
        {
            WorkshopPatches.Forget(info.m_nPublishedFileId.m_PublishedFileId);
        }

        private static void OnItemUpdated(RemoteStoragePublishedFileUpdated_t info)
        {
            WorkshopPatches.Forget(info.m_nPublishedFileId.m_PublishedFileId);
        }

        private static void OnItemSubscribed(RemoteStoragePublishedFileSubscribed_t info)
        {
            WorkshopPatches.Forget(info.m_nPublishedFileId.m_PublishedFileId);
        }

        private static void OnItemUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t info)
        {
            WorkshopPatches.Forget(info.m_nPublishedFileId.m_PublishedFileId);
        }
    }
}
