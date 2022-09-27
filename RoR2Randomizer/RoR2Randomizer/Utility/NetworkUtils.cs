using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
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
            ReadBits(reader, out bit1);
            bit2 = _sharedBitsArray[1];
        }

        public static void ReadBits(this NetworkReader reader, out bool bit1, out bool bit2, out bool bit3)
        {
            ReadBits(reader, out bit1, out bit2);
            bit3 = _sharedBitsArray[2];
        }
    }
}
