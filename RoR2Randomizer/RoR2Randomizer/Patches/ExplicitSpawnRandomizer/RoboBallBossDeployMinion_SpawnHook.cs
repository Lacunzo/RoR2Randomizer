using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class RoboBallBossDeployMinion_SpawnHook
    {
        static readonly ILContext.Manipulator _replaceMinionsManipulator = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeRoboBallBossMinions);

        static void Apply()
        {
            IL.EntityStates.RoboBallBoss.Weapon.DeployMinions.SummonMinion += _replaceMinionsManipulator;
        }

        static void Cleanup()
        {
            IL.EntityStates.RoboBallBoss.Weapon.DeployMinions.SummonMinion -= _replaceMinionsManipulator;
        }
    }
}
