using RoR2Randomizer.Utility;
using System;

namespace RoR2Randomizer.Extensions
{
    public static class DelegateExtensions
    {
        public static TryConvertDelegate<T> AddNullCheck<T>(this Func<T, T> converter) where T : class
        {
            return (T input, out T output) =>
            {
                output = converter(input);
                return output is not null;
            };
        }

        public static TryConvertToNextValue<T> ToConvertToNextValue<T>(this TryConvertDelegate<T> tryConvert)
        {
            return (ref T value) =>
            {
                return tryConvert(value, out value);
            };
        }
    }
}
