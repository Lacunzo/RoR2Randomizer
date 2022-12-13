using RoR2Randomizer.Utility;
using System;
using System.Runtime.CompilerServices;

namespace RoR2Randomizer.Extensions
{
    public static class IndexReplacementsCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasReplacement<T>(this in IndexReplacementsCollection instance, in T original) where T : Enum
        {
            return instance.HasReplacement((int)(object)original);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetReplacement<T>(this in IndexReplacementsCollection instance, in T original, out T replacement) where T : Enum
        {
            bool result = instance.TryGetReplacement((int)(object)original, out int replacementInt);
            replacement = (T)(object)replacementInt;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOriginal<T>(this in IndexReplacementsCollection instance, in T replacement) where T : Enum
        {
            return instance.HasOriginal((int)(object)replacement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetOriginal<T>(this in IndexReplacementsCollection instance, in T replacement, out T original) where T : Enum
        {
            bool result = instance.TryGetOriginal((int)(object)replacement, out int originalInt);
            original = (T)(object)originalInt;
            return result;
        }
    }
}
