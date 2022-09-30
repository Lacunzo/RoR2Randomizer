using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            ReplaceFireProjectileServerPrefab.Apply();
        }

        public static void Cleanup()
        {
            ReplaceFireProjectileServerPrefab.Cleanup();
        }
    }
}
