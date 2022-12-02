using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class VoidMegaCrabItemBehavior_SpawnHook
    {
        static readonly ILContext.Manipulator _replaceVoidAlliesManipulator = ExplicitSpawnRandomizerController.GetSimpleDirectorSpawnRequestHook(ConfigManager.ExplicitSpawnRandomizer.RandomizeZoeaVoidAllies);

        static void Apply()
        {
            IL.RoR2.VoidMegaCrabItemBehavior.FixedUpdate += _replaceVoidAlliesManipulator;
        }

        static void Cleanup()
        {
            IL.RoR2.VoidMegaCrabItemBehavior.FixedUpdate -= _replaceVoidAlliesManipulator;
        }
    }
}
