using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class BeetleQueenSummonEggs_SpawnHook
    {
        static readonly ILContext.Manipulator _replaceBeetleQueenGuardsManipulator = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeBeetleQueenSummonGuards);

        static void Apply()
        {
            IL.EntityStates.BeetleQueenMonster.SummonEggs.SummonEgg += _replaceBeetleQueenGuardsManipulator;
        }

        static void Cleanup()
        {
            IL.EntityStates.BeetleQueenMonster.SummonEggs.SummonEgg -= _replaceBeetleQueenGuardsManipulator;
        }
    }
}
