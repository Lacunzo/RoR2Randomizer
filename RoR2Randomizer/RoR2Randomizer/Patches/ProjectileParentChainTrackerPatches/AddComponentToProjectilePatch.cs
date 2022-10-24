using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class AddComponentToProjectilePatch
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileController.Awake += ProjectileController_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileController.Awake -= ProjectileController_Awake;
        }

        static void ProjectileController_Awake(On.RoR2.Projectile.ProjectileController.orig_Awake orig, RoR2.Projectile.ProjectileController self)
        {
            self.gameObject.AddComponent<ProjectileParentChainTracker>();
            orig(self);
        }
    }
}
