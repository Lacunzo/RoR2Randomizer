using HG;
using RoR2;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public static class NetworkUtils
    {
        static bool[] _sharedBitsArray = Array.Empty<bool>();

        static void readBitsIntoArray(NetworkReader reader, int amount)
        {
            ArrayUtils.EnsureCapacity(ref _sharedBitsArray, amount);
            reader.ReadBitArray(_sharedBitsArray, amount);
        }

        public static void WriteBits(this NetworkWriter writer, params bool[] bits)
        {
            writer.WriteBitArray(bits);
        }

        public static void ReadBits(this NetworkReader reader, out bool bit)
        {
            readBitsIntoArray(reader, 1);
            bit = _sharedBitsArray[0];
        }

        public static void ReadBits(this NetworkReader reader, out bool bit1, out bool bit2)
        {
            readBitsIntoArray(reader, 2);
            bit1 = _sharedBitsArray[0];
            bit2 = _sharedBitsArray[1];
        }

        public static void ReadBits(this NetworkReader reader, out bool bit1, out bool bit2, out bool bit3)
        {
            readBitsIntoArray(reader, 3);
            bit1 = _sharedBitsArray[0];
            bit2 = _sharedBitsArray[1];
            bit3 = _sharedBitsArray[2];
        }
    }
}
