using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    public static class ReplaceFireProjectileServerPrefab
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo -= ProjectileManager_FireProjectile_FireProjectileInfo;
        }

        static void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            if (ProjectileRandomizerController.TryReplaceFire(fireProjectileInfo, self.gameObject))
            {
                return;
            }

            orig(self, fireProjectileInfo);
        }
    }
}
