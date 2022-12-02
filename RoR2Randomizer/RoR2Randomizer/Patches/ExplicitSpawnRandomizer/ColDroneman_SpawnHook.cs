using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class ColDroneman_SpawnHook
    {
        static readonly ILContext.Manipulator _replaceColDronemanSpawnManipulator = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeDrones);

        static void Apply()
        {
            IL.RoR2.DroneWeaponsBehavior.TrySpawnDrone += _replaceColDronemanSpawnManipulator;
        }

        static void Cleanup()
        {
            IL.RoR2.DroneWeaponsBehavior.TrySpawnDrone -= _replaceColDronemanSpawnManipulator;
        }
    }
}
