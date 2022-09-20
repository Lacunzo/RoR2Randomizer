using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            Mithrix.BrotherSpeechDriver_ReplaceName.Apply();
            Mithrix.MithrixPhaseTracker.Apply();
            Mithrix.MithrixSpawnCardTracker.Apply();
            Mithrix.SpawnHook.Apply();
        }

        public static void Cleanup()
        {
            Mithrix.BrotherSpeechDriver_ReplaceName.Cleanup();
            Mithrix.MithrixPhaseTracker.Cleanup();
            Mithrix.MithrixSpawnCardTracker.Cleanup();
            Mithrix.SpawnHook.Cleanup();
        }
    }
}
