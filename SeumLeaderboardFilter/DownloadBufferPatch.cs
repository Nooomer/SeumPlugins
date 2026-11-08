using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace SeumLeaderboardFilter
{
    /// <summary>
    /// The shipped Assembly-CSharp.dll asks Steam for only 15 entries and
    /// allocates a 15-element downloadedScores array. That is enough for the
    /// vanilla board, but after this plugin hides rows there is nothing left to
    /// fill the visible list with, so the table shrinks (hide #1 and #2 and the
    /// board shows 3..15 instead of a full 15 rows).
    ///
    /// This patch lifts the request and the callback cap to DownloadBuffer.Size
    /// and makes sure the array can hold that many entries. The UI still copies
    /// at most 15 rows from the larger download, so the visible table keeps its
    /// normal size while the extra entries provide replacements for hidden
    /// rows. DownloadBuffer.ArraySize is deliberately larger than the request
    /// size so an already-modified game DLL (for example one that asks for 50
    /// itself) can never overflow the array.
    /// </summary>
    internal static class DownloadBuffer
    {
        internal const int Size = 50;
        internal const int ArraySize = 128;
    }

    [HarmonyPatch(typeof(SteamDownloadRequest), "downloadLeaderboardEntries")]
    internal static class DownloadRangePatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceFifteen(instructions);
        }

        internal static IEnumerable<CodeInstruction> ReplaceFifteen(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 15)
                {
                    instruction.operand = (sbyte)DownloadBuffer.Size;
                }
                else if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 15)
                {
                    instruction.operand = DownloadBuffer.Size;
                }

                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(SteamDownloadRequest), "leaderboardScoresDownloaded")]
    internal static class DownloadBufferPatch
    {
        [HarmonyPrefix]
        private static void Prefix(SteamDownloadRequest __instance)
        {
            if (__instance.downloadedScores == null
                || __instance.downloadedScores.Length < DownloadBuffer.ArraySize)
            {
                __instance.downloadedScores = new SteamScore[DownloadBuffer.ArraySize];
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // The shipped method caps scoreCount at 15; lift that cap to the
            // same buffer size as the request.
            return DownloadRangePatch.ReplaceFifteen(instructions);
        }
    }
}
