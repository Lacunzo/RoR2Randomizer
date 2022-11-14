using EntityStates.Engi.EngiWeapon;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class EngineerTurret_SpawnHook
    {
        static void Apply()
        {
            IL.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate += PlaceTurret_FixedUpdate;
            IL.RoR2.CharacterBody.HandleConstructTurret += CharacterBody_HandleConstructTurret;

            IL.EntityStates.Engi.EngiWeapon.PlaceTurret.GetPlacementInfo += PlaceTurret_GetPlacementInfo;
        }

        static void Cleanup()
        {
            IL.EntityStates.Engi.EngiWeapon.PlaceTurret.FixedUpdate -= PlaceTurret_FixedUpdate;
            IL.RoR2.CharacterBody.HandleConstructTurret -= CharacterBody_HandleConstructTurret;

            IL.EntityStates.Engi.EngiWeapon.PlaceTurret.GetPlacementInfo -= PlaceTurret_GetPlacementInfo;
        }

        static void PlaceTurret_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                                 x => x.MatchLdfld<PlaceTurret>(nameof(PlaceTurret.turretMasterPrefab)),
                                 x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => MasterCatalog.FindMasterIndex(default(GameObject))))))
            {
                c.EmitDelegate((MasterCatalog.MasterIndex original) =>
                {
                    MasterCatalog.MasterIndex replacement = ExplicitSpawnRandomizerController.GetSummonReplacement(original);
                    return replacement.isValid ? replacement : original;
                });
            }
        }

        static void CharacterBody_HandleConstructTurret(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<MasterSummon>(nameof(MasterSummon.Perform))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((CharacterMaster summoned) =>
                {
                    if (NetworkServer.active)
                    {
                        ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(summoned.gameObject);
                    }
                });
            }
        }

        static void PlaceTurret_GetPlacementInfo(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const float TURRET_RADUIS = 0.5f;
            const float TURRET_PLACE_DISTANCE = 2f;

            // Prevents player from falling into the ground after placing a turret
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(TURRET_PLACE_DISTANCE)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((float originalDistance, PlaceTurret instance) =>
                {
                    GameObject turretReplacementMasterObject = ExplicitSpawnRandomizerController.GetSummonReplacement(instance.turretMasterPrefab);
                    if (turretReplacementMasterObject && turretReplacementMasterObject.TryGetComponent<CharacterMaster>(out CharacterMaster replacementTurretMaster))
                    {
                        if (replacementTurretMaster.bodyPrefab && Caches.CharacterBodyRadius.TryGetValue(replacementTurretMaster.bodyPrefab, out float radius))
                        {
                            float result = radius + (TURRET_PLACE_DISTANCE - TURRET_RADUIS);
#if DEBUG
                            Log.Debug($"Override radius: {TURRET_PLACE_DISTANCE} -> {result}");
#endif
                            return Mathf.Max(originalDistance, result);
                        }
                    }

                    return originalDistance;
                });
            }

            // A On hook doesn't work for this method, probably due to the return type being private?
            c.Index = 0;
            while (c.TryGotoNext(x => x.MatchRet()))
            {
                c.EmitDelegate((PlaceTurret.PlacementInfo result) =>
                {
                    if (ExplicitSpawnRandomizerController.IsActive)
                    {
                        // Rotate turret to face away from the engineer
                        result.rotation *= Quaternion.Euler(0f, 180f, 0f);
                    }

                    return result;
                });

                c.Index++;
            }
        }
    }
}
