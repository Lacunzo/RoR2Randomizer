using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RoR2Randomizer.Extensions
{
    public static class IndexReplacementsCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasReplacement<T>(this IndexReplacementsCollection instance, T original) where T : Enum
        {
            return instance.HasReplacement((int)(object)original);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetReplacement<T>(this IndexReplacementsCollection instance, T original, out T replacement) where T : Enum
        {
            bool result = instance.TryGetReplacement((int)(object)original, out int replacementInt);
            replacement = (T)(object)replacementInt;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOriginal<T>(this IndexReplacementsCollection instance, T replacement) where T : Enum
        {
            return instance.HasOriginal((int)(object)replacement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetOriginal<T>(this IndexReplacementsCollection instance, T replacement, out T original) where T : Enum
        {
            bool result = instance.TryGetOriginal((int)(object)replacement, out int originalInt);
            original = (T)(object)originalInt;
            return result;
        }
    }
}
