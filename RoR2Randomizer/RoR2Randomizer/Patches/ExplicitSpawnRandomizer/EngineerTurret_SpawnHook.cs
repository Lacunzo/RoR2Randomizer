using EntityStates.Engi.EngiWeapon;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
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
            const string LOG_PREFIX = $"{nameof(EngineerTurret_SpawnHook)}.{nameof(PlaceTurret_FixedUpdate)} ";

            ILCursor c = new ILCursor(il);

            int numLocationsPatched = 0;
            while (c.TryGotoNext(MoveType.After,
                                 x => x.MatchLdfld<PlaceTurret>(nameof(PlaceTurret.turretMasterPrefab)),
                                 x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => MasterCatalog.FindMasterIndex(default(GameObject))))))
            {
                numLocationsPatched++;
                c.EmitDelegate((MasterCatalog.MasterIndex original) =>
                {
                    if (!ConfigManager.ExplicitSpawnRandomizer.RandomizeEngiTurrets)
                        return original;

                    MasterCatalog.MasterIndex replacement = ExplicitSpawnRandomizerController.GetSummonReplacement(original);
                    return replacement.isValid ? replacement : original;
                });
            }

            if (numLocationsPatched == 0)
            {
                Log.Warning($"{LOG_PREFIX}Failed to patch any locations for {nameof(PlaceTurret.FixedUpdate)}");
            }
#if DEBUG
            else
            {
                Log.Debug($"{LOG_PREFIX}Patched {numLocationsPatched} locations for {nameof(PlaceTurret.FixedUpdate)}");
            }
#endif
        }

        static void CharacterBody_HandleConstructTurret(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(EngineerTurret_SpawnHook)}.{nameof(CharacterBody_HandleConstructTurret)} ";

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<MasterSummon>(nameof(MasterSummon.Perform))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((CharacterMaster summoned) =>
                {
                    if (NetworkServer.active && ConfigManager.ExplicitSpawnRandomizer.RandomizeEngiTurrets)
                    {
                        ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(summoned.gameObject);
                    }
                });
            }
            else
            {
                Log.Warning($"{LOG_PREFIX}Failed to find patch the location");
            }
        }

        static void PlaceTurret_GetPlacementInfo(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(EngineerTurret_SpawnHook)}.{nameof(PlaceTurret_GetPlacementInfo)} ";

            ILCursor c = new ILCursor(il);

            const float TURRET_RADUIS = 0.5f;
            const float TURRET_PLACE_DISTANCE = 2f;

            int numLocationsPatched = 0;
            // Prevents player from falling into the ground after placing a turret
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(TURRET_PLACE_DISTANCE)))
            {
                numLocationsPatched++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((float originalDistance, PlaceTurret instance) =>
                {
                    if (ConfigManager.ExplicitSpawnRandomizer.RandomizeEngiTurrets)
                    {
                        GameObject turretReplacementMasterObject = ExplicitSpawnRandomizerController.GetSummonReplacement(instance.turretMasterPrefab);
                        if (turretReplacementMasterObject && turretReplacementMasterObject.TryGetComponent<CharacterMaster>(out CharacterMaster replacementTurretMaster))
                        {
                            if (replacementTurretMaster.bodyPrefab && Caches.CharacterBodyRadius.TryGetValue(replacementTurretMaster.bodyPrefab, out float radius))
                            {
                                float result = radius + (TURRET_PLACE_DISTANCE - TURRET_RADUIS);
#if DEBUG
                                Log.Debug($"{LOG_PREFIX}Override radius: {TURRET_PLACE_DISTANCE} -> {result}");
#endif
                                return Mathf.Max(originalDistance, result);
                            }
                        }
                    }

                    return originalDistance;
                });
            }

            if (numLocationsPatched == 0)
            {
                Log.Warning($"{LOG_PREFIX}Failed to patch any locations for radius override in {nameof(PlaceTurret.GetPlacementInfo)}");
            }
#if DEBUG
            else
            {
                Log.Debug($"{LOG_PREFIX}Patched {numLocationsPatched} locations for radius override in {nameof(PlaceTurret.GetPlacementInfo)}");
            }
#endif

            // A On hook doesn't work for this method, probably due to the return type being private?
            c.Index = 0;
            numLocationsPatched = 0;
            while (c.TryGotoNext(x => x.MatchRet()))
            {
                numLocationsPatched++;
                c.EmitDelegate((PlaceTurret.PlacementInfo result) =>
                {
                    if (ExplicitSpawnRandomizerController.IsActive && ConfigManager.ExplicitSpawnRandomizer.RandomizeEngiTurrets)
                    {
                        // Rotate turret to face away from the engineer
                        result.rotation *= Quaternion.Euler(0f, 180f, 0f);
                    }

                    return result;
                });

                c.Index++;
            }

            if (numLocationsPatched == 0)
            {
                Log.Warning($"{LOG_PREFIX}Failed to patch any locations for turret rotation in {nameof(PlaceTurret.GetPlacementInfo)}");
            }
#if DEBUG
            else
            {
                Log.Debug($"{LOG_PREFIX}Patched {numLocationsPatched} locations for turret rotation in {nameof(PlaceTurret.GetPlacementInfo)}");
            }
#endif
        }
    }
}
