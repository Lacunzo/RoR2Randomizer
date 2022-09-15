using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.CharacterRandomizer
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            Mithrix.BrotherSpeechDriver_ReplaceName.Apply();
            Mithrix.SpawnCardTracker.Apply();
            Mithrix.SpawnHook.Apply();
        }

        public static void Cleanup()
        {
            Mithrix.BrotherSpeechDriver_ReplaceName.Cleanup();
            Mithrix.SpawnCardTracker.Cleanup();
            Mithrix.SpawnHook.Cleanup();
        }
    }
}
