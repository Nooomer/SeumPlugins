using System;
using System.Reflection;
using HarmonyLib;

namespace SeumSteamOpt
{
    /// <summary>
    /// SteamManager is internal to the game assembly, so the one thing this plugin needs from it -
    /// "has SteamAPI.Init() succeeded" - is reached by reflection rather than by referencing it.
    /// </summary>
    internal static class SteamState
    {
        private static readonly MethodInfo InitializedGetter = ResolveInitialized();

        private static MethodInfo ResolveInitialized()
        {
            Type type = typeof(SeumSteam).Assembly.GetType("SteamManager", throwOnError: false);
            return type == null ? null : AccessTools.PropertyGetter(type, "Initialized");
        }

        /// <summary>
        /// False whenever Steam is not usable, including when the lookup itself failed. Every caller
        /// treats false as "do not touch the Steam API", so failing closed is the safe direction.
        /// </summary>
        internal static bool Initialized
        {
            get
            {
                if (InitializedGetter == null)
                {
                    return false;
                }

                try
                {
                    return (bool)InitializedGetter.Invoke(null, null);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
