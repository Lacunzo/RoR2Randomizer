using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.RandomizerController.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    public static class ReplaceFireProjectileServerPrefab
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectileServer += ProjectileManager_FireProjectileServer;

            IL.RoR2.Projectile.ProjectileManager.FireProjectileClient += ProjectileManager_FireProjectileClient;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileManager.FireProjectileServer -= ProjectileManager_FireProjectileServer;

            IL.RoR2.Projectile.ProjectileManager.FireProjectileClient -= ProjectileManager_FireProjectileClient;
        }

        static void ProjectileManager_FireProjectileServer(On.RoR2.Projectile.ProjectileManager.orig_FireProjectileServer orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo, NetworkConnection clientAuthorityOwner, ushort predictionId, double fastForwardTime)
        {
            if (NetworkServer.active)
            {
                ProjectileRandomizerController.TryOverrideProjectilePrefab(ref fireProjectileInfo.projectilePrefab);
            }

            orig(self, fireProjectileInfo, clientAuthorityOwner, predictionId, fastForwardTime);
        }

        static void ProjectileManager_FireProjectileClient(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // FireProjectileInfo fireProjectileInfo
            c.Emit(OpCodes.Ldarga, 1);
            c.EmitDelegate((ref FireProjectileInfo fireProjectileInfo) =>
            {
                if (!NetworkServer.active && NetworkClient.active)
                {
                    ProjectileRandomizerController.TryOverrideProjectilePrefab(ref fireProjectileInfo.projectilePrefab);
                }
            });

            if (c.TryGotoNext(x => x.MatchStfld<ProjectileManager.PlayerFireProjectileMessage>(nameof(ProjectileManager.PlayerFireProjectileMessage.prefabIndexPlusOne))))
            {
                c.EmitDelegate((uint prefabIndexPlusOne) =>
                {
                    int prefabIndex = RoR2.Util.UintToIntMinusOne(prefabIndexPlusOne);

                    if (ProjectileRandomizerController.TryGetOriginalProjectileIndex(prefabIndex, out int originalIndex))
                    {
                        prefabIndex = originalIndex;
                    }

                    return RoR2.Util.IntToUintPlusOne(prefabIndex);
                });
            }
        }
    }
}
