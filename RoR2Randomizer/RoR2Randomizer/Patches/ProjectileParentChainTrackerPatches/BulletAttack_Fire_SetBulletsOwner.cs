using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class BulletAttack_Fire_SetBulletsOwner
    {
        static void Apply()
        {
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
        }

        static void Cleanup()
        {
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
        }

        static void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, RoR2.BulletAttack self)
        {
            BulletAttackIdentifier identifier = BulletAttackCatalog.GetBulletAttackIdentifier(self);
            bool isValid = identifier.IsValid;
            if (isValid)
            {
                ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile = new ProjectileParentChainNode(identifier)
                {
                    Parent = ProjectileManager_InitializeProjectile_SetOwnerPatch.ResolveChainNodeForCurrentOwner()
                };
            }

            orig(self);

            if (isValid)
            {
                ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile = null;
            }
        }
    }
}
