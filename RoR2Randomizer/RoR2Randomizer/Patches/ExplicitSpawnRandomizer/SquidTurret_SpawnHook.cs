using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class SquidTurret_SpawnHook
    {
        static readonly ILContext.Manipulator GlobalEventManager_OnInteractionBegin = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeSquidTurrets);

        static void Apply()
        {
            IL.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
        }

        static void Cleanup()
        {
            IL.RoR2.GlobalEventManager.OnInteractionBegin -= GlobalEventManager_OnInteractionBegin;
        }
    }
}
