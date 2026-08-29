using System;
using System.Reflection;
using HarmonyLib;

namespace SeumSteamOpt
{
    /// <summary>
    /// Registers patches one at a time so a member the game renamed only costs that single patch
    /// instead of taking the whole plugin down with it.
    /// </summary>
    internal static class Patcher
    {
        internal static bool Patch(Harmony harmony, Type owner, Type target, string method,
            Type[] parameters = null, string prefix = null, string postfix = null)
        {
            string label = target.Name + "." + method;
            try
            {
                MethodBase original = parameters == null
                    ? AccessTools.Method(target, method)
                    : AccessTools.Method(target, method, parameters);

                if (original == null)
                {
                    Plugin.Log.LogWarning($"could not resolve '{label}', skipping.");
                    return false;
                }

                harmony.Patch(original,
                    prefix == null ? null : new HarmonyMethod(AccessTools.Method(owner, prefix)),
                    postfix == null ? null : new HarmonyMethod(AccessTools.Method(owner, postfix)));

                if (SteamOptConfig.VerboseLogging.Value)
                {
                    Plugin.Log.LogInfo($"patched {label}");
                }

                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"failed to patch '{label}': {e.Message}");
                return false;
            }
        }
    }
}
