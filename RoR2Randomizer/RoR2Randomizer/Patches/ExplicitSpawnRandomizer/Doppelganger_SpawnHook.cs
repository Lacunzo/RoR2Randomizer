using MonoMod.Cil;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class Doppelganger_SpawnHook
    {
        static readonly ILContext.Manipulator _replaceDoppelgangerSpawnManipulator = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeDoppelgangers);

        static void Apply()
        {
            IL.RoR2.Artifacts.DoppelgangerInvasionManager.CreateDoppelganger += _replaceDoppelgangerSpawnManipulator;
        }

        static void Cleanup()
        {
            IL.RoR2.Artifacts.DoppelgangerInvasionManager.CreateDoppelganger -= _replaceDoppelgangerSpawnManipulator;
        }
    }
}
