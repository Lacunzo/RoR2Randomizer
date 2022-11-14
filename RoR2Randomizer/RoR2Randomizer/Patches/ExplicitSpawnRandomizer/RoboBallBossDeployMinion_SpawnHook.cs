using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class RoboBallBossDeployMinion_SpawnHook
    {
        static void Apply()
        {
            IL.EntityStates.RoboBallBoss.Weapon.DeployMinions.SummonMinion += DeployMinions_SummonMinion;
        }

        static void Cleanup()
        {
            IL.EntityStates.RoboBallBoss.Weapon.DeployMinions.SummonMinion -= DeployMinions_SummonMinion;
        }

        static void DeployMinions_SummonMinion(ILContext il)
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
