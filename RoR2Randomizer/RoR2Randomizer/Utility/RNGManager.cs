using RoR2;
using RoR2Randomizer.Extensions;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public static class RNGManager
    {
        // Deliberately not using a static readonly field for RandomizerServerRNG here, since this class isn't accessed until a run is started, the static constructor won't run until then, meaning the RunSpecific callbacks are registered *during* the onStart foreach loop which causes an InvalidOperationException.

        public static RunSpecific<Xoroshiro128Plus> RandomizerServerRNG { get; private set; }

        [SystemInitializer]
        static void Init()
        {
            RandomizerServerRNG = new RunSpecific<Xoroshiro128Plus>((out Xoroshiro128Plus result) =>
            {
                if (NetworkServer.active)
                {
                    result = new Xoroshiro128Plus(Run.instance.runRNG.Next());
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }, 10);
        }
    }
}
