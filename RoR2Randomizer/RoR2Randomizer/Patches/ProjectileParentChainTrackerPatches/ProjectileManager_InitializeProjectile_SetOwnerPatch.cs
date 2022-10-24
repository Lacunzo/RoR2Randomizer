using RoR2.Projectile;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileManager_InitializeProjectile_SetOwnerPatch
    {
        internal static GameObject OwnerOfNextProjectile;

        static void Apply()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;

            On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;

            On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
        }

        static void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            orig(self, fireProjectileInfo);
            OwnerOfNextProjectile = null;
        }

        static void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
        {
            if (OwnerOfNextProjectile && projectileController.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker parentChainTracker))
            {
                parentChainTracker.TrySetParent(OwnerOfNextProjectile);
                OwnerOfNextProjectile = null;
            }

            orig(projectileController, fireProjectileInfo);
        }
    }
}
