using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeumDiscordRPC
{
    public class DiscordSceneTracker : MonoBehaviour
    {
        private void Awake()
        {
            Object.DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("[Discord] Scene loaded: " + scene.name);
            switch (scene.name)
            {
                case "MainMenu":
                    DiscordController.UpdatePresence("Main Menu", "Idling", "", "");
                    break;
                case "LevelSelector":
                    DiscordController.UpdatePresence("Level Selection", "Choosing level", "", "");
                    break;
                case "Speedrun":
                    DiscordController.UpdatePresence("Speedrun Mode", "Beating records", "", "");
                    break;
                case "EndlessMode":
                    DiscordController.UpdatePresence("Endless Mode", "Running...", "", "");
                    break;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
