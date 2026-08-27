using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SeumDiscordRPC
{
    // Ships discord_game_sdk.dll embedded inside this assembly instead of requiring the
    // user to drop it into the game's own folders. DllImport("discord_game_sdk") only
    // searches the process's standard native search paths (game dir, System32, PATH...),
    // none of which include this plugin's own folder - so we extract the embedded copy
    // next to this DLL and LoadLibrary() it by full path once, up front. Windows then
    // resolves every later implicit load of "discord_game_sdk.dll" (what DllImport does
    // under the hood) to that already-loaded module instead of searching for it again.
    internal static class NativeLibraryLoader
    {
        private const string ResourceName = "SeumDiscordRPC.Native.discord_game_sdk.dll";
        private const string FileName = "discord_game_sdk.dll";

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        internal static void EnsureDiscordGameSdkLoaded()
        {
            try
            {
                string targetPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    FileName);

                ExtractIfNeeded(targetPath);

                if (LoadLibrary(targetPath) == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Plugin.Logger.LogError($"[Discord] Failed to load {FileName} from '{targetPath}' (Win32 error {error}).");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[Discord] Failed to prepare {FileName}: {ex}");
            }
        }

        private static void ExtractIfNeeded(string targetPath)
        {
            using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (resourceStream == null)
            {
                Plugin.Logger.LogError($"[Discord] Embedded resource '{ResourceName}' not found.");
                return;
            }

            if (File.Exists(targetPath) && new FileInfo(targetPath).Length == resourceStream.Length)
            {
                return;
            }

            using FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }
    }
}
