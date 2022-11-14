using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.ChildTransformAdditions;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class HookProjectileImpactFix
    {
        static void Apply()
        {
            On.RoR2.Projectile.HookProjectileImpact.Start += HookProjectileImpact_Start;

            On.RoR2.Projectile.HookProjectileImpact.Reel += HookProjectileImpact_Reel;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.HookProjectileImpact.Start -= HookProjectileImpact_Start;

            On.RoR2.Projectile.HookProjectileImpact.Reel -= HookProjectileImpact_Reel;
        }

        static void HookProjectileImpact_Start(On.RoR2.Projectile.HookProjectileImpact.orig_Start orig, HookProjectileImpact self)
        {
            Transform ownerTransform = self.GetComponent<ProjectileController>().owner.transform;
            if (ownerTransform)
            {
                ModelLocator modelLocator = ownerTransform.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    Transform modelTransform = modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                        if (childLocator)
                        {
                            CustomChildTransformManager.AutoAddChildTransform(ownerTransform.GetComponent<CharacterBody>(), childLocator, self.attachmentString);
                        }
                    }
                }
            }

            orig(self);
        }

        static bool HookProjectileImpact_Reel(On.RoR2.Projectile.HookProjectileImpact.orig_Reel orig, HookProjectileImpact self)
        {
            if (!self.ownerTransform || !self.victim)
            {
                self.hookState = HookProjectileImpact.HookState.ReelFail;
                return false;
            }

            return orig(self);
        }
    }
}
