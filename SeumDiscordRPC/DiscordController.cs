using System;
using Discord;
using UnityEngine;

namespace SeumDiscordRPC
{
    public class DiscordController : MonoBehaviour
    {
        private global::Discord.Discord discord;

        private static DiscordController instance;

        private bool isInitialized;

        private float reconnectTimer;

        private const float RECONNECT_INTERVAL = 15f;

        private const long CLIENT_ID = 425442824134328320L;

        private void Update()
        {
            if (!isInitialized)
            {
                reconnectTimer += Time.deltaTime;
                if (reconnectTimer >= RECONNECT_INTERVAL)
                {
                    reconnectTimer = 0f;
                    TryInitializeDiscord();
                }
                return;
            }

            if (discord == null)
            {
                return;
            }

            try
            {
                discord.RunCallbacks();
            }
            catch (ResultException ex)
            {
                Debug.LogWarning("[Discord] RunCallbacks failed: " + ex.Message + ". Discord might be closed.");
                isInitialized = false;
                DisposeDiscord();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Discord] Unexpected error in RunCallbacks: " + ex.Message);
                isInitialized = false;
                DisposeDiscord();
            }
        }

        public static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            GameObject obj = new GameObject("DiscordController");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            instance = obj.AddComponent<DiscordController>();
            Debug.Log("[Discord] Controller initialized and added to scene");
        }

        private void Awake()
        {
            Debug.Log("[Discord] Awake called");
            TryInitializeDiscord();
        }

        private void OnDestroy()
        {
            DisposeDiscord();
            isInitialized = false;
        }

        public static void UpdatePresence(string state, string details, string largeText, string smallText)
        {
            if (instance != null && instance.discord != null && instance.isInitialized)
            {
                instance.SetPresence(state, details, largeText, smallText);
            }
        }

        private void SetPresence(string state, string details, string largeText, string smallText)
        {
            try
            {
                ActivityManager activityManager = discord.GetActivityManager();
                Activity activity = new Activity
                {
                    State = state,
                    Details = details,
                    Assets = new ActivityAssets
                    {
                        LargeImage = "https://cdn.discordapp.com/app-icons/425442824134328320/c6ad13512430edcf89937c575961ffe8.png",
                        LargeText = largeText ?? state,
                        SmallImage = "small_status",
                        SmallText = smallText ?? details
                    }
                };
                activityManager.UpdateActivity(activity, delegate (Result result)
                {
                    if (result != Result.Ok)
                    {
                        Debug.LogWarning(string.Format("[Discord] UpdateActivity failed: {0}", result));
                    }
                });
            }
            catch (ResultException ex)
            {
                Debug.LogWarning("[Discord] Error updating presence: " + ex.Message + ". Marking as not initialized.");
                isInitialized = false;
                DisposeDiscord();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Discord] Error updating presence: " + ex.Message);
            }
        }

        private void TryInitializeDiscord()
        {
            try
            {
                discord = new global::Discord.Discord(CLIENT_ID, 2uL);
                Debug.Log(string.Format("[Discord] Init successful: {0}", discord));
                isInitialized = true;
                reconnectTimer = 0f;
                SetPresence("Starting game", "Loading...", "", "");
            }
            catch (ResultException ex)
            {
                Debug.LogWarning("[Discord] Init failed (Discord probably not running): " + ex.Message);
                isInitialized = false;
                discord = null;
            }
            catch (Exception ex)
            {
                Debug.LogError("[Discord] Init failed: " + ex.Message);
                isInitialized = false;
                discord = null;
            }
        }

        private void DisposeDiscord()
        {
            if (discord == null)
            {
                return;
            }

            try
            {
                discord.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Discord] Error during dispose: " + ex.Message);
            }
            finally
            {
                discord = null;
            }
        }

        private void OnApplicationQuit()
        {
            DisposeDiscord();
            isInitialized = false;
        }
    }
}
