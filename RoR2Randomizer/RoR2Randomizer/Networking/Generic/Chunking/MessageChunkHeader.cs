using RoR2.Networking;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Generic.Chunking
{
    public readonly struct MessageChunkHeader
    {
        public const int SIZE = (sizeof(byte) * 16) // MessageID
                              + sizeof(byte); // ChunkIndex

        public readonly Guid MessageID;
        public readonly byte ChunkIndex;

        public MessageChunkHeader(Guid messageID, byte chunkIndex)
        {
            MessageID = messageID;
            ChunkIndex = chunkIndex;
        }

        public MessageChunkHeader(NetworkReader reader) : this(reader.ReadGuid(), reader.ReadByte())
        {
        }

        public readonly void Serialize(NetworkWriter writer)
        {
            writer.WriteGuid(MessageID);
            writer.Write(ChunkIndex);
        }
    }
}
