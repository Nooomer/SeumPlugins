using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace SeumLeaderboardFilter
{
    /// <summary>
    /// Local SteamID64 blacklist. Only #1 leaderboard entries are filtered against it.
    ///
    /// The file is watched by timestamp, so adding or removing a player and then
    /// reopening a leaderboard is enough — no game restart. Lines may carry a
    /// trailing '#' comment; anything that is not a SteamID64 is reported once
    /// and ignored.
    /// </summary>
    internal static class Blacklist
    {
        private const string DefaultContents =
            "# SeumLeaderboardFilter — who to hide from #1.\n"
            + "# One SteamID64 per line. A trailing # comment is allowed.\n"
            + "# Only that player's #1 entries are removed; lower places are untouched.\n"
            + "\n"
            + "76561198351858610  # keaz\n"
            + "76561198336109574  # bruh\n";

        private static readonly HashSet<ulong> Ids = new HashSet<ulong>();
        private static string path;
        private static DateTime lastWriteUtc = DateTime.MinValue;
        private static bool initialized;

        internal static int Count => Ids.Count;
        internal static string Path => path;

        internal static void Initialize(string configuredPath)
        {
            path = Resolve(configuredPath);
            initialized = true;
            Reload(true);
        }

        internal static bool Contains(ulong steamId)
        {
            return Ids.Contains(steamId);
        }

        /// <summary>
        /// Called once per downloaded leaderboard. Reading the timestamp is cheap;
        /// re-parsing only happens when the file actually changed.
        /// </summary>
        internal static void RefreshIfChanged()
        {
            if (!initialized || string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                if (!File.Exists(path))
                {
                    // The user may have deleted the file to clear the list; do not
                    // silently recreate it, just forget the old players.
                    if (Ids.Count != 0)
                    {
                        Ids.Clear();
                        lastWriteUtc = DateTime.MinValue;
                        Plugin.Log.LogInfo("Blacklist file disappeared; blacklist cleared.");
                    }
                    return;
                }

                DateTime stamp = File.GetLastWriteTimeUtc(path);
                if (stamp != lastWriteUtc)
                {
                    Reload(false);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not check blacklist file: " + e.Message);
            }
        }

        private static void Reload(bool createIfMissing)
        {
            try
            {
                if (!File.Exists(path))
                {
                    if (!createIfMissing)
                    {
                        return;
                    }

                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    File.WriteAllText(path, DefaultContents);
                    Plugin.Log.LogInfo("Created default blacklist: " + path);
                }

                lastWriteUtc = File.GetLastWriteTimeUtc(path);
                var parsed = new HashSet<ulong>();
                int lineNumber = 0;

                foreach (string raw in File.ReadAllLines(path))
                {
                    lineNumber++;
                    string line = raw;
                    int comment = line.IndexOf('#');
                    if (comment >= 0)
                    {
                        line = line.Substring(0, comment);
                    }

                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    ulong id;
                    if (ulong.TryParse(line, out id))
                    {
                        parsed.Add(id);
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"Blacklist line {lineNumber} is not a SteamID64: {line}");
                    }
                }

                Ids.Clear();
                foreach (ulong id in parsed)
                {
                    Ids.Add(id);
                }

                Plugin.Log.LogInfo($"Blacklist reloaded: {Ids.Count} player(s).");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not read blacklist file: " + e.Message);
            }
        }

        private static string Resolve(string configuredPath)
        {
            if (string.IsNullOrEmpty(configuredPath))
            {
                configuredPath = "SeumLeaderboardFilter.blacklist.txt";
            }

            return System.IO.Path.IsPathRooted(configuredPath)
                ? configuredPath
                : System.IO.Path.Combine(Paths.ConfigPath, configuredPath);
        }
    }
}
