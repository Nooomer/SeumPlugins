using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace SeumReplay
{
    /// <summary>
    /// An on-disk cache of downloaded replay blobs, keyed by the Steam UGC handle the leaderboard
    /// entry points at.
    ///
    /// The handle is what makes this safe: uploading a new personal best publishes a new UGC file
    /// and the leaderboard entry starts pointing at the new handle, so a cached blob is only ever
    /// served for the exact run it was downloaded for. The game already caches a decoded session on
    /// the <see cref="Score"/> object, but leaderboard rows are rebuilt on every refresh and thrown
    /// away on level change, so watching the same world record twice costs two downloads.
    ///
    /// Every operation here is best-effort: a cache that cannot be read, written or trimmed must
    /// never take the game down with it, so failures are logged and the download path carries on.
    /// </summary>
    internal static class ReplayCache
    {
        private const string DirectoryName = "SeumReplay";
        private const string Extension = ".rpl";

        private static string cacheDirectory;
        private static bool available;

        internal static void Init()
        {
            if (!ReplayConfig.DiskCache.Value)
            {
                return;
            }

            try
            {
                cacheDirectory = System.IO.Path.Combine(Paths.CachePath, DirectoryName);
                Directory.CreateDirectory(cacheDirectory);
                available = true;
                TrimOnStartup();
            }
            catch (Exception e)
            {
                available = false;
                Plugin.Log.LogWarning("Replay disk cache disabled, could not open " + cacheDirectory + ": " + e.Message);
            }
        }

        internal static bool TryLoad(ulong ugcHandle, out byte[] data)
        {
            data = null;
            if (!available || ugcHandle == ulong.MaxValue)
            {
                return false;
            }

            try
            {
                string path = PathFor(ugcHandle);
                if (!File.Exists(path))
                {
                    return false;
                }

                data = File.ReadAllBytes(path);
                if (data.Length < 8)
                {
                    Remove(ugcHandle);
                    data = null;
                    return false;
                }

                // Doubles as the LRU timestamp: trimming drops the least recently used files.
                File.SetLastAccessTimeUtc(path, DateTime.UtcNow);

                if (ReplayConfig.VerboseLogging.Value)
                {
                    Plugin.Log.LogInfo("Replay cache hit for UGC " + ugcHandle + " (" + data.Length + " B).");
                }

                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Replay cache read failed for UGC " + ugcHandle + ": " + e.Message);
                data = null;
                return false;
            }
        }

        internal static void Store(ulong ugcHandle, byte[] data)
        {
            if (!available || ugcHandle == ulong.MaxValue || data == null || data.Length == 0)
            {
                return;
            }

            try
            {
                string path = PathFor(ugcHandle);
                string temporary = path + ".tmp";
                File.WriteAllBytes(temporary, data);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(temporary, path);
                TrimToBudget();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Replay cache write failed for UGC " + ugcHandle + ": " + e.Message);
            }
        }

        internal static void Remove(ulong ugcHandle)
        {
            if (!available)
            {
                return;
            }

            try
            {
                string path = PathFor(ugcHandle);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Replay cache delete failed for UGC " + ugcHandle + ": " + e.Message);
            }
        }

        private static string PathFor(ulong ugcHandle)
        {
            return System.IO.Path.Combine(cacheDirectory, ugcHandle.ToString("x16") + Extension);
        }

        private static void TrimOnStartup()
        {
            int maxAgeDays = ReplayConfig.CacheMaxAgeDays.Value;
            if (maxAgeDays > 0)
            {
                DateTime cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
                foreach (FileInfo file in Files())
                {
                    if (LastUse(file) < cutoff)
                    {
                        Delete(file);
                    }
                }
            }

            TrimToBudget();
        }

        private static void TrimToBudget()
        {
            long budget = (long)ReplayConfig.CacheBudgetMegabytes.Value * 1024L * 1024L;

            List<FileInfo> files = Files();
            long total = 0;
            foreach (FileInfo file in files)
            {
                total += file.Length;
            }

            if (total <= budget)
            {
                return;
            }

            files.Sort((a, b) => LastUse(a).CompareTo(LastUse(b)));
            foreach (FileInfo file in files)
            {
                if (total <= budget)
                {
                    break;
                }

                long size = file.Length;
                if (Delete(file))
                {
                    total -= size;
                }
            }
        }

        private static List<FileInfo> Files()
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                DirectoryInfo directory = new DirectoryInfo(cacheDirectory);
                if (directory.Exists)
                {
                    files.AddRange(directory.GetFiles("*" + Extension));
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not enumerate the replay cache: " + e.Message);
            }

            return files;
        }

        /// <summary>
        /// Access time is the LRU signal, but some filesystems (and Windows with last-access
        /// updates switched off) leave it stale or in the future, so fall back to write time.
        /// </summary>
        private static DateTime LastUse(FileInfo file)
        {
            DateTime accessed = file.LastAccessTimeUtc;
            DateTime written = file.LastWriteTimeUtc;
            return accessed > written ? accessed : written;
        }

        private static bool Delete(FileInfo file)
        {
            try
            {
                file.Delete();
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not evict " + file.Name + " from the replay cache: " + e.Message);
                return false;
            }
        }
    }
}
