using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.MultiEntityStatePatches
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            EntityStateExitOnDestroy.Apply();
            SetStateOuterPatch.Apply();
            InitializeMultiStatePatch.Apply();
        }

        public static void Cleanup()
        {
            EntityStateExitOnDestroy.Cleanup();
            SetStateOuterPatch.Cleanup();
            InitializeMultiStatePatch.Cleanup();
        }
    }
}
