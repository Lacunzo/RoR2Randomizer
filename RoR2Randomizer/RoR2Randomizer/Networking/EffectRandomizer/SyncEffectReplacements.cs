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
        public delegate void OnCompleteMessageReceivedDelegate(ReplacementDictionary<EffectIndex> effectReplacements);
        public static event OnCompleteMessageReceivedDelegate OnCompleteMessageReceived;

        static readonly Dictionary<Guid, EffectReplacementMessageChunk?[]> _recordedChunksByID = new Dictionary<Guid, EffectReplacementMessageChunk?[]>();

        const int MAX_EFFECTS_PER_MESSAGE = 200;

        readonly struct EffectReplacementMessageChunk
        {
            public readonly Guid MessageID;
            public readonly ulong MessageIndex;
            public readonly ulong MessageCount;
            public readonly KeyValuePair<EffectIndex, EffectIndex>[] EffectIndexPairs;

            public EffectReplacementMessageChunk(Guid messageID, ulong messageIndex, ulong messageCount, KeyValuePair<EffectIndex, EffectIndex>[] effectIndexPairs)
            {
                MessageID = messageID;
                MessageIndex = messageIndex;
                MessageCount = messageCount;
                EffectIndexPairs = effectIndexPairs;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.WriteGuid(MessageID);

                writer.WritePackedUInt64(MessageIndex);
                writer.WritePackedUInt64(MessageCount);

                writer.WritePackedUInt64((ulong)EffectIndexPairs.LongLength);
                foreach (KeyValuePair<EffectIndex, EffectIndex> pair in EffectIndexPairs)
                {
                    writer.WriteEffectIndex(pair.Key);
                    writer.WriteEffectIndex(pair.Value);
                }
            }

            public override string ToString()
            {
                return $"{MessageID}, {MessageIndex + 1}/{MessageCount} ({EffectIndexPairs.Length})";
            }

            public static EffectReplacementMessageChunk Deserialize(NetworkReader reader)
            {
                Guid messageID = reader.ReadGuid();

                ulong messageIndex = reader.ReadPackedUInt64();
                ulong messageCount = reader.ReadPackedUInt64();

                ulong length = reader.ReadPackedUInt64();
                KeyValuePair<EffectIndex, EffectIndex>[] effectIndexPairs = new KeyValuePair<EffectIndex, EffectIndex>[length];

                for (ulong i = 0; i < length; i++)
                {
                    effectIndexPairs[i] = new KeyValuePair<EffectIndex, EffectIndex>(reader.ReadEffectIndex(), reader.ReadEffectIndex());
                }

                return new EffectReplacementMessageChunk(messageID, messageIndex, messageCount, effectIndexPairs);
            }

            public static ReplacementDictionary<EffectIndex> BuildReplacementDictionary(EffectReplacementMessageChunk?[] messageChunks)
            {
                Dictionary<EffectIndex, EffectIndex> dict = new Dictionary<EffectIndex, EffectIndex>();

                foreach (EffectReplacementMessageChunk? chunk in messageChunks)
                {
                    if (!chunk.HasValue)
                    {
                        Log.Warning("Chunk has no value!");
                        continue;
                    }

                    foreach (KeyValuePair<EffectIndex, EffectIndex> pair in chunk.Value.EffectIndexPairs)
                    {
                        dict.Add(pair.Key, pair.Value);
                    }
                }

                return new ReplacementDictionary<EffectIndex>(dict);
            }
        }

        EffectReplacementMessageChunk _chunk;

        public SyncEffectReplacements()
        {
        }

        SyncEffectReplacements(Guid messageID, ulong messageIndex, ulong messageCount, ReplacementDictionary<EffectIndex> effectReplacements)
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

        public static void SendToClients(ReplacementDictionary<EffectIndex> effectReplacements)
        {
            Guid messageID = Guid.NewGuid();

            ulong messageCount = (ulong)Mathf.Ceil(effectReplacements.Count / (float)MAX_EFFECTS_PER_MESSAGE);

            for (ulong i = 0; i < messageCount; i++)
            {
                SyncEffectReplacements message = new SyncEffectReplacements(messageID, i, messageCount, effectReplacements);
                message.Send(NetworkDestination.Clients);

#if DEBUG
                Log.Debug($"Sent effect replacement chunk {message._chunk} to clients");
#endif
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
            Log.Debug($"{nameof(SyncEffectReplacements)} received");
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

                    OnCompleteMessageReceived?.Invoke(EffectReplacementMessageChunk.BuildReplacementDictionary(recordedChunks));
                }
            }
        }
    }
}
