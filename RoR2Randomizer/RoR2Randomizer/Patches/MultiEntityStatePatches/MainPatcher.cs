using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.MultiEntityStatePatches
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            SetStateOuterPatch.Apply();
            SetSubStatesPatch.Apply();
        }

        public static void Cleanup()
        {
            SetStateOuterPatch.Cleanup();
            SetSubStatesPatch.Cleanup();
        }
    }
}
