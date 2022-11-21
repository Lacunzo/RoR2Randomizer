using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes
{
    // ProjectileParentTether NullRefs if it has no owner (tries to access owner.transform without checking owner first), which causes a *lot* of logspam

    [PatchClass]
    static class ProjectileParentTether_FixedUpdate_NullRefFix
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileParentTether.FixedUpdate += ProjectileParentTether_FixedUpdate;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileParentTether.FixedUpdate -= ProjectileParentTether_FixedUpdate;
        }

        static void ProjectileParentTether_FixedUpdate(On.RoR2.Projectile.ProjectileParentTether.orig_FixedUpdate orig, RoR2.Projectile.ProjectileParentTether self)
        {
            if (!self.projectileController || !self.projectileController.owner)
                return;

            orig(self);
        }
    }
}
