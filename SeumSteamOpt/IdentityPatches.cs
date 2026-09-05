using HarmonyLib;

namespace SeumSteamOpt
{
    /// <summary>
    /// SeumSteam is a scene-embedded MonoBehaviour rather than a DontDestroyOnLoad singleton, so its
    /// Start() - and therefore SeumSteam.init() - reruns every time the scene it lives in loads,
    /// including every level restart during a practice session. init() reads the local player's
    /// persona name and Steam id and requests stats every single time, even though it already sets
    /// SeumSteam.initialized to true the first time it succeeds and never consults that flag again.
    /// None of GetPersonaName, GetSteamID or RequestCurrentStats's answer can change mid-process, so
    /// this is a clean case of the game having exactly the guard it needs and not using it.
    /// </summary>
    internal static class IdentityPatches
    {
        internal static void Apply(Harmony harmony)
        {
            if (!SteamOptConfig.CacheSteamIdentity.Value)
            {
                return;
            }

            Patcher.Patch(harmony, typeof(IdentityPatches), typeof(SeumSteam), "init",
                prefix: nameof(InitPrefix));
        }

        private static bool InitPrefix()
        {
            if (!SeumSteam.initialized)
            {
                return true;
            }

            Counters.Add(ref Counters.IdentityReads, 1);
            return false;
        }
    }
}
