using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes
{
    // Fixed NullRefs in ProjectileParentTether Update methods, which cause lots of logspam if the owner is killed while it is active

    [PatchClass]
    static class ProjectileParentTether_NullRefFix
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileParentTether.FixedUpdate += ProjectileParentTether_FixedUpdate;
            On.RoR2.Projectile.ProjectileParentTether.UpdateTetherGraphic += ProjectileParentTether_UpdateTetherGraphic;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileParentTether.FixedUpdate -= ProjectileParentTether_FixedUpdate;
            On.RoR2.Projectile.ProjectileParentTether.UpdateTetherGraphic -= ProjectileParentTether_UpdateTetherGraphic;
        }

        // NullRefs if it has no owner (tries to access owner.transform without checking owner first)
        static void ProjectileParentTether_FixedUpdate(On.RoR2.Projectile.ProjectileParentTether.orig_FixedUpdate orig, RoR2.Projectile.ProjectileParentTether self)
        {
            if (!self.projectileController || !self.projectileController.owner)
                return;

            orig(self);
        }

        // NullRefs if it has no owner (calls GetAimRay which tries to access owner.transform without checking owner first)
        static void ProjectileParentTether_UpdateTetherGraphic(On.RoR2.Projectile.ProjectileParentTether.orig_UpdateTetherGraphic orig, RoR2.Projectile.ProjectileParentTether self)
        {
            if (!self.projectileController || !self.projectileController.owner)
                return;

            orig(self);
        }
    }
}
