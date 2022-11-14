using HG;
using UnityEngine.Networking;

namespace RoR2Randomizer.Extensions
{
    public static class NetworkingExtensions
    {
        public static SerializableSystemType ReadSerializableType(this NetworkReader reader)
        {
            return new SerializableSystemType() { assemblyQualifiedName = reader.ReadString() };
        }

        public static void WriteSerializableType(this NetworkWriter writer, SerializableSystemType serializableSystemType)
        {
            writer.Write(serializableSystemType.assemblyQualifiedName);
        }
    }
}
