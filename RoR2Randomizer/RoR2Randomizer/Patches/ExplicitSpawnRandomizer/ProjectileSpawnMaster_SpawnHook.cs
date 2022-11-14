using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class ProjectileSpawnMaster_SpawnHook
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileSpawnMaster.SpawnMaster += ProjectileSpawnMaster_SpawnMaster;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileSpawnMaster.SpawnMaster += ProjectileSpawnMaster_SpawnMaster;
        }

        static void ProjectileSpawnMaster_SpawnMaster(ILContext il)
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
