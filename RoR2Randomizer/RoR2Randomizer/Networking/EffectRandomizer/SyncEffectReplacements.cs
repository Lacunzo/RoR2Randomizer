using HG;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.EffectRandomizer
{
    public sealed class SyncEffectReplacements : INetMessage
    {
        public delegate void OnCompleteMessageReceivedDelegate(IndexReplacementsCollection effectReplacements);
        public static event OnCompleteMessageReceivedDelegate OnCompleteMessageReceived;

        static readonly Dictionary<Guid, EffectReplacementMessageChunk?[]> _recordedChunksByID = new Dictionary<Guid, EffectReplacementMessageChunk?[]>();

        const int MAX_EFFECTS_PER_MESSAGE = 400;

        readonly struct EffectReplacementMessageChunk
        {
            public readonly Guid MessageID;
            public readonly ulong MessageIndex;
            public readonly ulong MessageCount;
            public readonly int[] EffectIndexReplacements;

            public EffectReplacementMessageChunk(Guid messageID, ulong messageIndex, ulong messageCount, int[] effectIndexReplacements)
            {
                MessageID = messageID;
                MessageIndex = messageIndex;
                MessageCount = messageCount;
                EffectIndexReplacements = effectIndexReplacements;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.WriteGuid(MessageID);

                writer.WritePackedUInt64(MessageIndex);
                writer.WritePackedUInt64(MessageCount);

                writer.WritePackedUInt64((ulong)EffectIndexReplacements.LongLength);
                foreach (int effectIndex in EffectIndexReplacements)
                {
                    writer.WriteEffectIndex((EffectIndex)effectIndex);
                }
            }

            public override string ToString()
            {
                return $"{MessageID}, {MessageIndex + 1}/{MessageCount} ({EffectIndexReplacements.Length})";
            }

            public static EffectReplacementMessageChunk Deserialize(NetworkReader reader)
            {
                Guid messageID = reader.ReadGuid();

                ulong messageIndex = reader.ReadPackedUInt64();
                ulong messageCount = reader.ReadPackedUInt64();

                ulong length = reader.ReadPackedUInt64();
                int[] effectIndexReplacements = new int[length];
                for (ulong i = 0; i < length; i++)
                {
                    effectIndexReplacements[i] = (int)reader.ReadEffectIndex();
                }

                return new EffectReplacementMessageChunk(messageID, messageIndex, messageCount, effectIndexReplacements);
            }

            public static IndexReplacementsCollection BuildReplacementCollection(EffectReplacementMessageChunk?[] messageChunks)
            {
                const string LOG_PREFIX = $"{nameof(SyncEffectReplacements)}+{nameof(EffectReplacementMessageChunk)}.{nameof(BuildReplacementCollection)} ";

                // It won't matter if the array is too large, IndexReplacementsCollection will deal with it :)
                int[] replacementIndices = new int[messageChunks.Length * MAX_EFFECTS_PER_MESSAGE];

                int currentArrayIndex = 0;
                foreach (EffectReplacementMessageChunk? nChunk in messageChunks)
                {
                    if (nChunk.HasValue)
                    {
                        EffectReplacementMessageChunk chunk = nChunk.Value;

                        int chunkReplacementsLength = chunk.EffectIndexReplacements.Length;

                        // Just in case the predicted length isn't enough
                        ArrayUtils.EnsureCapacity(ref replacementIndices, currentArrayIndex + chunkReplacementsLength);

                        Array.Copy(chunk.EffectIndexReplacements, 0, replacementIndices, currentArrayIndex, chunkReplacementsLength);
                        currentArrayIndex += chunkReplacementsLength;
                    }
                    else
                    {
                        Log.Warning(LOG_PREFIX + $"null chunk!");
                    }
                }

                return new IndexReplacementsCollection(replacementIndices, currentArrayIndex);
            }
        }

        EffectReplacementMessageChunk _chunk;

        public SyncEffectReplacements()
        {
        }

        SyncEffectReplacements(Guid messageID, ulong messageIndex, ulong messageCount, IndexReplacementsCollection effectReplacements)
        {
            static IEnumerable<T> getRange<T>(IEnumerable<T> collection, ulong startIndex, int count)
            {
                ulong i = 0;
                foreach (T item in collection)
                {
                    if (i >= startIndex && i - startIndex < (ulong)count)
                    {
                        yield return item;
                    }

                    i++;
                }
            }

            _chunk = new EffectReplacementMessageChunk(messageID, messageIndex, messageCount, getRange(effectReplacements, messageIndex * MAX_EFFECTS_PER_MESSAGE, MAX_EFFECTS_PER_MESSAGE).ToArray());
        }

        public static IEnumerable<SyncEffectReplacements> CreateMessagesFor(IndexReplacementsCollection effectReplacements)
        {
            Guid messageID = Guid.NewGuid();

            ulong messageCount = (ulong)Mathf.Ceil(effectReplacements.Length / (float)MAX_EFFECTS_PER_MESSAGE);

            for (ulong i = 0; i < messageCount; i++)
            {
                SyncEffectReplacements message = new SyncEffectReplacements(messageID, i, messageCount, effectReplacements);

#if DEBUG
                Log.Debug($"Created effect replacement chunk {message._chunk}");
#endif

                yield return message;
            }
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            _chunk.Serialize(writer);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _chunk = EffectReplacementMessageChunk.Deserialize(reader);
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncEffectReplacements)} received ({_chunk})");
#endif

            if (!NetworkServer.active && NetworkClient.active)
            {
#if DEBUG
                Log.Debug($"{nameof(SyncEffectReplacements)} received as client, recording chunk");
#endif

                recordChunk(_chunk);
            }
#if DEBUG
            else
            {
                Log.Debug($"{nameof(SyncEffectReplacements)} received as server, skipping");
            }
#endif
        }

        static void recordChunk(EffectReplacementMessageChunk messageChunk)
        {
#if DEBUG
            Log.Debug($"Recording chunk {messageChunk}");
#endif

            if (!_recordedChunksByID.TryGetValue(messageChunk.MessageID, out EffectReplacementMessageChunk?[] recordedChunks))
            {
                recordedChunks = new EffectReplacementMessageChunk?[messageChunk.MessageCount];
                _recordedChunksByID.Add(messageChunk.MessageID, recordedChunks);
            }

            ref EffectReplacementMessageChunk? chunkToRecord = ref recordedChunks[messageChunk.MessageIndex];
            if (chunkToRecord.HasValue)
            {
                Log.Warning($"Duplicate chunk recorded for {messageChunk}");
            }
            else
            {
#if DEBUG
                Log.Debug($"Recorded chunk {messageChunk}");
#endif

                chunkToRecord = messageChunk;

                if (recordedChunks.All(c => c.HasValue))
                {
#if DEBUG
                    Log.Debug($"Received all chunks for message {messageChunk.MessageID}");
#endif

                    _recordedChunksByID.Remove(messageChunk.MessageID);

                    OnCompleteMessageReceived?.Invoke(EffectReplacementMessageChunk.BuildReplacementCollection(recordedChunks));
                }
            }
        }
    }
}
