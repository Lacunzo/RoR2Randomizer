using RoR2.Projectile;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileExplosion_FireChild_SetChildOwner
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileExplosion.FireChild += ProjectileExplosion_FireChild;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileExplosion.FireChild -= ProjectileExplosion_FireChild;
        }

        static void ProjectileExplosion_FireChild(On.RoR2.Projectile.ProjectileExplosion.orig_FireChild orig, ProjectileExplosion self)
        {
            ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = self.gameObject;
            orig(self);
            ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = null;
        }
    }
}
