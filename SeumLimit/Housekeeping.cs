using System;
using System.IO;
using System.Linq;
using BepInEx;
using IOPath = System.IO.Path;

namespace SeumLimit
{
    /// <summary>
    /// Keeps BepInEx/cache/SeumLimit from growing without bound. Runs once at startup:
    /// results of older analysis versions are dropped (they are never shown again, the key
    /// carries the version), and only the newest reviews are kept.
    /// </summary>
    internal static class Housekeeping
    {
        internal static void Run()
        {
            string root = IOPath.Combine(Paths.CachePath, "SeumLimit");
            try
            {
                string results = IOPath.Combine(root, "results");
                if (Directory.Exists(results))
                {
                    string current = "v" + TopRuns.AnalysisVersion + "_";
                    foreach (string file in Directory.GetFiles(results, "*.txt"))
                    {
                        if (!IOPath.GetFileName(file).StartsWith(current, StringComparison.Ordinal))
                        {
                            File.Delete(file);
                        }
                    }
                }

                string reviews = IOPath.Combine(root, "reviews");
                if (Directory.Exists(reviews))
                {
                    foreach (FileInfo old in new DirectoryInfo(reviews).GetFiles("*.txt")
                        .OrderByDescending(f => f.LastWriteTimeUtc)
                        .Skip(LimitConfig.KeepReviews.Value))
                    {
                        old.Delete();
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Cache cleanup failed: " + e.Message);
            }
        }
    }
}
