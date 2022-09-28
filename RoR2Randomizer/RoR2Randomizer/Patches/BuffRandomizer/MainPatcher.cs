using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            BuffIndexPatch.Apply();
        }

        public static void Cleanup()
        {
            BuffIndexPatch.Cleanup();
        }
    }
}
