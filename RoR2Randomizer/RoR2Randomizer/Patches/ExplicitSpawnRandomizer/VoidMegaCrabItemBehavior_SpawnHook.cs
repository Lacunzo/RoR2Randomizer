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
        static void Apply()
        {
            IL.RoR2.VoidMegaCrabItemBehavior.FixedUpdate += VoidMegaCrabItemBehavior_FixedUpdate;
        }

        static void Cleanup()
        {
            IL.RoR2.VoidMegaCrabItemBehavior.FixedUpdate -= VoidMegaCrabItemBehavior_FixedUpdate;
        }

        static void VoidMegaCrabItemBehavior_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate(static (DirectorSpawnRequest spawnRequest) =>
                {
                    if (ConfigManager.ExplicitSpawnRandomizer.RandomizeZoeaVoidAllies)
                    {
                        ExplicitSpawnRandomizerController.TryReplaceDirectorSpawnRequest(spawnRequest);
                    }
                });
            }
        }
    }
}
