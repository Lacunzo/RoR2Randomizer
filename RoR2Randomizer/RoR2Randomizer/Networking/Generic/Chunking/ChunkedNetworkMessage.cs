using HG;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Generic.Chunking
{
    public abstract class ChunkedNetworkMessage : NetworkMessageBase
    {
        public const uint MAX_PACKET_SIZE = 1024;

        class ConstructingMessage
        {
            public readonly Guid MessageID;

            byte[][] _collectedMessageData;

            SerializableSystemType? _messageType;

            public ConstructingMessage(Guid messageID)
            {
                MessageID = messageID;
            }

            public void HandleHeader(ChunkedMessageHeader header)
            {
                const string LOG_PREFIX = $"{nameof(ChunkedNetworkMessage)}+{nameof(ConstructingMessage)}.{nameof(HandleHeader)} ";

                if (_collectedMessageData == null)
                {
                    _collectedMessageData = new byte[header.MessageCount][];
                }
                else
                {
                    Array.Resize(ref _collectedMessageData, header.MessageCount);
                }

                _messageType = header.CompleteMessageType;

#if DEBUG
                string typeName;
                if (_messageType.HasValue)
                {
                    typeName = ((Type)_messageType.Value)?.Name;
                }
                else
                {
                    typeName = "???";
                }

                Log.Debug(LOG_PREFIX + $"({typeName}) received header");
#endif

                refreshMessages();
            }

            public void CollectChunk(MessageChunk chunk)
            {
                const string LOG_PREFIX = $"{nameof(ChunkedNetworkMessage)}+{nameof(ConstructingMessage)}.{nameof(CollectChunk)} ";

                if (_collectedMessageData == null)
                {
                    _collectedMessageData = new byte[chunk.Header.ChunkIndex + 1][];
                }
                else
                {
                    ArrayUtils.EnsureCapacity(ref _collectedMessageData, chunk.Header.ChunkIndex + 1);
                }

                _collectedMessageData[chunk.Header.ChunkIndex] = chunk.Data;

#if DEBUG
                string typeName;
                if (_messageType.HasValue)
                {
                    typeName = ((Type)_messageType.Value)?.Name;
                }
                else
                {
                    typeName = "???";
                }

                Log.Debug(LOG_PREFIX + $"({typeName}) received chunk {chunk.Header.ChunkIndex + 1}/{_collectedMessageData.Length}");
#endif

                refreshMessages();
            }

            void refreshMessages()
            {
                if (_messageType.HasValue && _collectedMessageData.All(static m => m != null))
                {
                    onFullMessageReceived();
                }
            }

            void onFullMessageReceived()
            {
                const string LOG_PREFIX = $"{nameof(ChunkedNetworkMessage)}+{nameof(ConstructingMessage)}.{nameof(onFullMessageReceived)} ";

                object message;
                try
                {
                    Type messageType = (Type)_messageType.Value;
                    message = Activator.CreateInstance(messageType);
                }
                catch (Exception e)
                {
                    Log.Warning(LOG_PREFIX + $"exception while creating message instance: {e}");
                    message = null;
                }

                if (message != null)
                {
                    receiveMessage(message);
                }

                _constructingMessages.Remove(MessageID);
            }

            void receiveMessage(object message)
            {
                const string LOG_PREFIX = $"{nameof(ChunkedNetworkMessage)}+{nameof(ConstructingMessage)}.{nameof(receiveMessage)} ";

                if (message is INetMessage netMessage)
                {
                    byte[] completeMessageData = new byte[_collectedMessageData.Sum(static m => m.Length)];

                    int currentIndex = 0;
                    foreach (byte[] chunkData in _collectedMessageData)
                    {
                        Array.Copy(chunkData, 0, completeMessageData, currentIndex, chunkData.Length);
                        currentIndex += chunkData.Length;
                    }

                    NetworkReader reader = new NetworkReader(completeMessageData);
                    netMessage.Deserialize(reader);
                    netMessage.OnReceived();
                }
                else
                {
                    Log.Warning(LOG_PREFIX + $"message is not {nameof(INetMessage)} ({message})");
                }
            }
        }

        static readonly Dictionary<Guid, ConstructingMessage> _constructingMessages = new Dictionary<Guid, ConstructingMessage>();
        static ConstructingMessage getOrCreateConstructingMessage(Guid messageID)
        {
            if (!_constructingMessages.TryGetValue(messageID, out ConstructingMessage constructingMessage))
            {
                constructingMessage = new ConstructingMessage(messageID);
                _constructingMessages.Add(messageID, constructingMessage);
            }

            return constructingMessage;
        }

        static void handleReceiveChunkHeader(ChunkedMessageHeader header)
        {
            Guid messageID = header.ID;
            ConstructingMessage constructingMessage = getOrCreateConstructingMessage(messageID);
            constructingMessage.HandleHeader(header);
        }

        static void handleReceiveChunk(MessageChunk chunk)
        {
            Guid messageID = chunk.Header.MessageID;
            ConstructingMessage constructingMessage = getOrCreateConstructingMessage(messageID);
            constructingMessage.CollectChunk(chunk);
        }

        static ChunkedNetworkMessage()
        {
            Run.onRunDestroyGlobal += static _ =>
            {
                _constructingMessages.Clear();
            };
        }

        bool trySendChunked(Action<NetworkMessageBase> sendMessageFunc)
        {
            const string LOG_PREFIX = $"{nameof(ChunkedNetworkMessage)}.{nameof(trySendChunked)} ";

            NetworkWriter writer = new NetworkWriter();
            Serialize(writer);

            byte[] messageBytes = writer.AsArray();
            int totalMessageSize = messageBytes.Length;
            if (totalMessageSize >= MAX_PACKET_SIZE)
            {
                byte chunkCount = (byte)Mathf.CeilToInt(totalMessageSize / (float)(MAX_PACKET_SIZE + MessageChunkHeader.SIZE));

                Guid messageID = Guid.NewGuid();

                ChunkedMessageHeader chunkHeaderMessage = new ChunkedMessageHeader(messageID, chunkCount, (SerializableSystemType)GetType());
#if DEBUG
                Log.Debug(LOG_PREFIX + $"({GetType().Name}) sending header");
#endif
                sendMessageFunc(chunkHeaderMessage);

                for (byte i = 0; i < chunkCount; i++)
                {
                    MessageChunk chunkMessage = new MessageChunk(new MessageChunkHeader(messageID, i), messageBytes, i * MAX_PACKET_SIZE, MAX_PACKET_SIZE);
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"({GetType().Name}) sending chunk {i + 1}/{chunkCount}");
#endif
                    sendMessageFunc(chunkMessage);
                }

                return true;
            }

            return false;
        }

        public override void SendTo(NetworkConnection connection)
        {
            if (!trySendChunked(m => m.SendTo(connection)))
            {
                base.SendTo(connection);
            }
        }

        public override void SendTo(NetworkDestination destination)
        {
            if (!trySendChunked(m => m.SendTo(destination)))
            {
                base.SendTo(destination);
            }
        }

        internal class ChunkedMessageHeader : NetworkMessageBase
        {
            internal Guid ID { get; private set; }
            internal byte MessageCount { get; private set; }
            internal SerializableSystemType CompleteMessageType { get; private set; }

            public ChunkedMessageHeader()
            {
            }

            public ChunkedMessageHeader(Guid id, byte messageCount, SerializableSystemType completeMessageType)
            {
                ID = id;
                MessageCount = messageCount;
                CompleteMessageType = completeMessageType;
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.WriteGuid(ID);
                writer.Write(MessageCount);
                writer.WriteSerializableType(CompleteMessageType);
            }

            public override void Deserialize(NetworkReader reader)
            {
                ID = reader.ReadGuid();
                MessageCount = reader.ReadByte();
                CompleteMessageType = reader.ReadSerializableType();
            }

            public override void OnReceived()
            {
                handleReceiveChunkHeader(this);
            }
        }

        internal class MessageChunk : NetworkMessageBase
        {
            internal MessageChunkHeader Header { get; private set; }
            internal byte[] Data { get; private set; }

            readonly uint? _startIndex;
            readonly uint? _count;

            public MessageChunk()
            {
                _startIndex = null;
                _count = null;
            }

            public MessageChunk(MessageChunkHeader header, byte[] data, uint startIndex, uint count)
            {
                Header = header;

                int chunkSize = Mathf.Min((int)count, data.Length - (int)startIndex);
                Data = new byte[count];
                Array.Copy(data, startIndex, Data, 0, chunkSize);

                _startIndex = startIndex;
                _count = count;
            }

            public override void Serialize(NetworkWriter writer)
            {
                Header.Serialize(writer);

                if (!_startIndex.HasValue || !_count.HasValue)
                {
                    throw new InvalidOperationException($"Attempting to serialize {nameof(ChunkedNetworkMessage)}+{nameof(MessageChunk)} with null {nameof(_startIndex)} or {nameof(_count)}");
                }

                writer.WriteBytesFull(Data);
            }

            public override void Deserialize(NetworkReader reader)
            {
                Header = new MessageChunkHeader(reader);

                uint chunkSize = reader.ReadPackedUInt32();
                Data = reader.ReadBytes((int)chunkSize);
            }

            public override void OnReceived()
            {
                handleReceiveChunk(this);
            }
        }
    }
}
