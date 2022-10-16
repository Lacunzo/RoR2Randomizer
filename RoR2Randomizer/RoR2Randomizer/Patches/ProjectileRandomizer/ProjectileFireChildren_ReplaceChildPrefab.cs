using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class ProjectileFireChildren_ReplaceChildPrefab
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileFireChildren.Start += ProjectileFireChildren_Start;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileFireChildren.Start -= ProjectileFireChildren_Start;
        }

        static void ProjectileFireChildren_Start(On.RoR2.Projectile.ProjectileFireChildren.orig_Start orig, ProjectileFireChildren self)
        {
            if (self.childProjectilePrefab)
            {
                ProjectileRandomizerController.TryOverrideProjectilePrefab(ref self.childProjectilePrefab);
            }

            orig(self);
        }
    }
}
