using RoR2.Projectile;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileManager_InitializeProjectile_SetOwnerPatch
    {
        internal static GameObject OwnerOfNextProjectile;

        internal static ProjectileParentChainNode BulletOwnerNodeOfNextProjectile;

        internal static ProjectileParentChainNode ResolveChainNodeForCurrentOwner()
        {
            if (BulletOwnerNodeOfNextProjectile != null)
                return BulletOwnerNodeOfNextProjectile;

            if (OwnerOfNextProjectile && OwnerOfNextProjectile.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker parentChainTracker))
                return parentChainTracker.ChainNode;

            return null;
        }

        static void Apply()
        {
            On.RoR2.Projectile.ProjectileController.Awake += ProjectileController_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileController.Awake -= ProjectileController_Awake;
        }

        static void ProjectileController_Awake(On.RoR2.Projectile.ProjectileController.orig_Awake orig, ProjectileController self)
        {
            ProjectileParentChainTracker parentChainTracker = self.gameObject.AddComponent<ProjectileParentChainTracker>();
            if (OwnerOfNextProjectile)
            {
                parentChainTracker.TrySetParent(OwnerOfNextProjectile);
                OwnerOfNextProjectile = null;
            }
            else if (BulletOwnerNodeOfNextProjectile != null)
            {
                parentChainTracker.SetParent(BulletOwnerNodeOfNextProjectile);
            }

            orig(self);
        }
    }
}
