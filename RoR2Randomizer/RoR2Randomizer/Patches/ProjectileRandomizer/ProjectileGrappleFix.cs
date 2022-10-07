using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class ProjectileGrappleFix
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileManager.InitializeProjectile -= ProjectileManager_InitializeProjectile;
        }

        static void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
        {
            orig(projectileController, fireProjectileInfo);

            if (ProjectileRandomizerController.TryGetOriginalProjectilePrefab(fireProjectileInfo.projectilePrefab, out GameObject originalProjectilePrefab))
            {
                if (originalProjectilePrefab.GetComponent<ProjectileGrappleController>() && !projectileController.GetComponent<ProjectileGrappleController>())
                {
                    const string STATE_MACHINE_NAME = "Hook";

                    EntityStateMachine hookStateMachine = EntityStateMachine.FindByCustomName(projectileController.owner, STATE_MACHINE_NAME);
                    if (hookStateMachine)
                    {
                        if (hookStateMachine.state is EntityStates.Loader.FireHook fireHook)
                        {
                            fireHook.SetHookReference(projectileController.gameObject);
                        }
                    }
                    else
                    {
                        Log.Warning($"Tried to initialize grapple projectile, but owner has no '{STATE_MACHINE_NAME}' state machine");
                    }
                }
            }
        }
    }
}
