using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeumReplay
{
    /// <summary>
    /// A replacement reader for the replay blob format (version 2).
    ///
    /// The wire format is flat: a header, then frameCount frames, then eventCount events, then
    /// projectileDataCount vectors. The game only chunks them into arrays of 60 / 100 / 20 once
    /// they are in memory, and <c>Replay.unpackReplay</c> conflates the two: it derives the length
    /// of the last chunk from <c>count % chunkSize</c>, which is 0 - not chunkSize - whenever the
    /// count divides evenly. When that happens the reader silently skips a whole chunk of records
    /// and every field after it is read from the wrong offset, so the replay either explodes on a
    /// bogus count or plays back as noise.
    ///
    /// This reader walks the records instead of the chunks, so the tail case disappears, and it
    /// reads straight out of the byte array rather than through a BinaryReader over a MemoryStream.
    /// Nothing about the format changes - recording and uploading are untouched, and a blob written
    /// by an unmodded client (or read by one) still means exactly what it meant before.
    /// </summary>
    internal static class ReplayCodec
    {
        internal const int Version = 2;

        private const int FrameChunk = 60;
        private const int EventChunk = 100;
        private const int ProjectileChunk = 20;

        private const int FrameBytes = 24;      // Vector3 position + rotationX + rotationY + forwardVelocity
        private const int EventBytes = 12;      // int type + int data + float timestamp
        private const int ProjectileBytes = 12; // Vector3

        /// <summary>Set when the last unpack had to clamp a count to the bytes that arrived.</summary>
        internal static bool LastUnpackWasTruncated;

        internal static Replay.ReplaySession Unpack(byte[] data)
        {
            LastUnpackWasTruncated = false;

            if (data == null || data.Length < 8)
            {
                Plugin.Log.LogWarning("Replay blob is empty or too short to hold a header.");
                return null;
            }

            if (!BitConverter.IsLittleEndian)
            {
                // BinaryWriter is little-endian everywhere, so on a big-endian runtime this
                // shortcut would be wrong. Nothing SEUM ships on is, but let the stock reader
                // have it rather than produce silent nonsense.
                return null;
            }

            int offset = 0;
            int version = ReadInt(data, ref offset);
            if (version != Version)
            {
                Plugin.Log.LogWarning("Unknown version of replay: " + version);
                return null;
            }

            long started = DateTime.UtcNow.Ticks;

            Replay.ReplaySession session = new Replay.ReplaySession();
            session.waitTime = ReadFloat(data, ref offset);

            int frameCount = Clamp(ReadInt(data, ref offset), data.Length - offset, FrameBytes, "frames");
            if (frameCount < 0)
            {
                return null;
            }

            session.frameCount = frameCount;
            session.frames = new List<Replay.ReplayFullFrame[]>((frameCount + FrameChunk - 1) / FrameChunk);
            Replay.ReplayFullFrame[] frameChunk = null;
            for (int i = 0; i < frameCount; i++)
            {
                if (i % FrameChunk == 0)
                {
                    frameChunk = new Replay.ReplayFullFrame[FrameChunk];
                    session.frames.Add(frameChunk);
                }

                int slot = i % FrameChunk;
                frameChunk[slot].position = new Vector3(
                    ReadFloat(data, ref offset),
                    ReadFloat(data, ref offset),
                    ReadFloat(data, ref offset));
                frameChunk[slot].rotationX = ReadFloat(data, ref offset);
                frameChunk[slot].rotationY = ReadFloat(data, ref offset);
                frameChunk[slot].forwardVelocity = ReadFloat(data, ref offset);
            }

            int eventCount = Clamp(ReadIntOrZero(data, ref offset), data.Length - offset, EventBytes, "events");
            if (eventCount < 0)
            {
                return null;
            }

            session.eventCount = eventCount;
            session.events = new List<Replay.ReplayEventRecord[]>((eventCount + EventChunk - 1) / EventChunk);
            Replay.ReplayEventRecord[] eventChunk = null;
            for (int i = 0; i < eventCount; i++)
            {
                if (i % EventChunk == 0)
                {
                    eventChunk = new Replay.ReplayEventRecord[EventChunk];
                    session.events.Add(eventChunk);
                }

                int slot = i % EventChunk;
                eventChunk[slot].type = ReadInt(data, ref offset);
                eventChunk[slot].data = ReadInt(data, ref offset);
                eventChunk[slot].timestamp = ReadFloat(data, ref offset);
            }

            int projectileCount = Clamp(ReadIntOrZero(data, ref offset), data.Length - offset, ProjectileBytes, "projectile data");
            if (projectileCount < 0)
            {
                return null;
            }

            session.projectileDataCount = projectileCount;
            session.projectileData = new List<Vector3[]>((projectileCount + ProjectileChunk - 1) / ProjectileChunk);
            Vector3[] projectileChunk = null;
            for (int i = 0; i < projectileCount; i++)
            {
                if (i % ProjectileChunk == 0)
                {
                    projectileChunk = new Vector3[ProjectileChunk];
                    session.projectileData.Add(projectileChunk);
                }

                projectileChunk[i % ProjectileChunk] = new Vector3(
                    ReadFloat(data, ref offset),
                    ReadFloat(data, ref offset),
                    ReadFloat(data, ref offset));
            }

            if (ReplayConfig.VerboseLogging.Value)
            {
                double ms = (DateTime.UtcNow.Ticks - started) / (double)TimeSpan.TicksPerMillisecond;
                Plugin.Log.LogInfo(string.Format(
                    "Unpacked replay: {0} B, {1} frames, {2} events, {3} projectile entries in {4:0.00} ms.",
                    data.Length, frameCount, eventCount, projectileCount, ms));
            }

            return session;
        }

        /// <summary>
        /// Trims a declared record count to what the buffer can actually supply, and returns -1
        /// when the blob has to be given up on.
        ///
        /// The game uploads <c>MemoryStream.GetBuffer()</c>, so a blob normally carries slack zero
        /// bytes past its real end and a small over-read is survivable - but a count that runs past
        /// the buffer means the blob is damaged, and under the stock code that throws inside a
        /// Steam callback, which is what leaves the UI stuck on "Loading..." forever.
        /// </summary>
        private static int Clamp(int count, int bytesAvailable, int recordBytes, string what)
        {
            if (count < 0)
            {
                Plugin.Log.LogWarning("Replay declares a negative " + what + " count (" + count
                    + "); treating the replay as unreadable.");
                return -1;
            }

            long needed = (long)count * recordBytes;
            if (needed <= bytesAvailable)
            {
                return count;
            }

            if (!ReplayConfig.TolerateTruncated.Value)
            {
                Plugin.Log.LogWarning("Replay is short by " + (needed - bytesAvailable) + " B of " + what + ".");
                return -1;
            }

            int fits = bytesAvailable / recordBytes;
            LastUnpackWasTruncated = true;
            Plugin.Log.LogWarning("Replay declares " + count + " " + what + " but only " + fits
                + " fit in the blob; playing the part that arrived.");
            return fits;
        }

        private static int ReadInt(byte[] data, ref int offset)
        {
            int value = BitConverter.ToInt32(data, offset);
            offset += 4;
            return value;
        }

        /// <summary>
        /// Section counts sit right after a variable-length section, so a damaged blob can end
        /// exactly on one. Report that as an empty section rather than an IndexOutOfRange.
        /// </summary>
        private static int ReadIntOrZero(byte[] data, ref int offset)
        {
            if (offset + 4 > data.Length)
            {
                LastUnpackWasTruncated = true;
                offset = data.Length;
                return 0;
            }

            return ReadInt(data, ref offset);
        }

        private static float ReadFloat(byte[] data, ref int offset)
        {
            float value = BitConverter.ToSingle(data, offset);
            offset += 4;
            return value;
        }
    }
}
