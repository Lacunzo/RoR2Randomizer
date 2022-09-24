using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            GenericScriptedSpawnHook.Apply();

            new Mithrix.MithrixPhaseTracker().ApplyPatches();
            Mithrix.BrotherSpeechDriver_ReplaceName.Apply();
            Mithrix.MithrixSpawnCardTracker.Apply();

            new Voidling.VoidlingPhaseTracker().ApplyPatches();
        }

        public static void Cleanup()
        {
            GenericScriptedSpawnHook.Cleanup();

            Mithrix.MithrixPhaseTracker.Instance?.CleanupPatches();
            Mithrix.BrotherSpeechDriver_ReplaceName.Cleanup();
            Mithrix.MithrixSpawnCardTracker.Cleanup();

            Voidling.VoidlingPhaseTracker.Instance?.CleanupPatches();
        }
    }
}
