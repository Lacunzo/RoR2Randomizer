using MonoMod.Cil;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class ParentEgg_HatchSpawnHook
    {
        static readonly ILContext.Manipulator Hatch_DoHatch = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeAncestralPods);

        static void Apply()
        {
            IL.EntityStates.ParentEgg.Hatch.DoHatch += Hatch_DoHatch;
        }

        static void Cleanup()
        {
            IL.EntityStates.ParentEgg.Hatch.DoHatch -= Hatch_DoHatch;
        }
    }
}
