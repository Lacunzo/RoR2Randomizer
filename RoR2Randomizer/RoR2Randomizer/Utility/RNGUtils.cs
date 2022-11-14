using RoR2;
using RoR2Randomizer.Extensions;

namespace RoR2Randomizer.Utility
{
    public static class RNGUtils
    {
        public static int RangeInt(int min, int max)
        {
            if (Run.instance && Run.instance.runRNG != null)
            {
                return Run.instance.runRNG.RangeInt(min, max);
            }
            else
            {
                return UnityEngine.Random.Range(min, max);
            }
        }

        public static T Choose<T>(params T[] options)
        {
            return options.GetRandomOrDefault();
        }

        public static float NormalizedFloat
        {
            get
            {
                if (Run.instance && Run.instance.runRNG != null)
                {
                    return Run.instance.runRNG.nextNormalizedFloat;
                }
                else
                {
                    return UnityEngine.Random.value;
                }
            }
        }
    }
}
