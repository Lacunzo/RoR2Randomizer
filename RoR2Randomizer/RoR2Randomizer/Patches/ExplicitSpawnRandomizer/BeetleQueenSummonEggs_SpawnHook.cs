using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class BeetleQueenSummonEggs_SpawnHook
    {
        static void Apply()
        {
            IL.EntityStates.BeetleQueenMonster.SummonEggs.SummonEgg += SummonEggs_SummonEgg;
        }

        static void Cleanup()
        {
            IL.EntityStates.BeetleQueenMonster.SummonEggs.SummonEgg -= SummonEggs_SummonEgg;
        }

        static void SummonEggs_SummonEgg(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate(ExplicitSpawnRandomizerController.ReplaceDirectorSpawnRequest);
            }
        }
    }
}
