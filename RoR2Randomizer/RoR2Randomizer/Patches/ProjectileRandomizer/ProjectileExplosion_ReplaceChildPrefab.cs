using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class ProjectileExplosion_ReplaceChildPrefab
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileExplosion.Awake += ProjectileExplosion_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileExplosion.Awake -= ProjectileExplosion_Awake;
        }

        static void ProjectileExplosion_Awake(On.RoR2.Projectile.ProjectileExplosion.orig_Awake orig, ProjectileExplosion self)
        {
            if (self.fireChildren)
            {
                ProjectileRandomizerController.TryOverrideProjectilePrefab(ref self.childrenProjectilePrefab);
            }

            orig(self);
        }
    }
}
