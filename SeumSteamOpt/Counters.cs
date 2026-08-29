using System.Diagnostics;
using System.Threading;

namespace SeumSteamOpt
{
    /// <summary>
    /// How many Steam calls each patch has taken off the wire. This is the only way to tell whether
    /// the plugin is doing anything on a given save, so it is always counted - the config only
    /// decides how often the numbers are printed.
    /// </summary>
    internal static class Counters
    {
        /// <summary>FindOrCreateLeaderboard calls answered from the handle cache.</summary>
        internal static long LeaderboardFinds;

        /// <summary>Leaderboard download requests never queued, thanks to the refresh cooldown.</summary>
        internal static long LeaderboardDownloads;

        internal static long PersonaNames;
        internal static long UserInfoRequests;
        internal static long AchievementReads;
        internal static long ItemStates;

        /// <summary>Workshop UGC queries deferred past the main menu or skipped as empty.</summary>
        internal static long WorkshopQueries;

        internal static void Add(ref long counter, long amount)
        {
            Interlocked.Add(ref counter, amount);
        }

        internal static long Total =>
            Interlocked.Read(ref LeaderboardFinds)
            + Interlocked.Read(ref LeaderboardDownloads)
            + Interlocked.Read(ref PersonaNames)
            + Interlocked.Read(ref UserInfoRequests)
            + Interlocked.Read(ref AchievementReads)
            + Interlocked.Read(ref ItemStates)
            + Interlocked.Read(ref WorkshopQueries);

        internal static string Summary()
        {
            return string.Format(
                "Steam calls avoided so far: {0} "
                + "(leaderboard lookups {1}, leaderboard downloads {2}, "
                + "persona names {3}, user-info requests {4}, "
                + "achievement reads {5}, workshop item states {6}, workshop queries {7})",
                Total,
                Interlocked.Read(ref LeaderboardFinds),
                Interlocked.Read(ref LeaderboardDownloads),
                Interlocked.Read(ref PersonaNames),
                Interlocked.Read(ref UserInfoRequests),
                Interlocked.Read(ref AchievementReads),
                Interlocked.Read(ref ItemStates),
                Interlocked.Read(ref WorkshopQueries));
        }
    }

    /// <summary>
    /// Time source for the caches. Deliberately not <c>Time.unscaledTime</c>: the patched Steamworks
    /// entry points are reachable from other plugins' worker threads, and Unity's time properties
    /// throw off the main thread.
    /// </summary>
    internal static class Clock
    {
        private static readonly Stopwatch Watch = Stopwatch.StartNew();

        internal static float Now => (float)Watch.Elapsed.TotalSeconds;
    }
}
