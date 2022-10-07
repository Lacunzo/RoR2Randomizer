using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class GlobalOnDeath_SpawnHook
    {
        static void Apply()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        static void Cleanup()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GlobalEventManager_OnCharacterDeath;
        }

        static void GlobalEventManager_OnCharacterDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] findCursors;

            // Malachite Urchin
            if (c.TryFindNext(out findCursors,
                              x => x.MatchLdstr("Prefabs/CharacterMasters/UrchinTurretMaster"),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => LegacyResourcesAPI.Load<GameObject>(default)))))
            {
                ILCursor last = findCursors[findCursors.Length - 1];
                last.Index++;

                last.EmitDelegate<Func<GameObject, GameObject>>(ExplicitSpawnRandomizerController.GetSummonReplacement);
            }

            // Soul wisp
            c.Index = 0;
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(GlobalEventManager.CommonAssets), nameof(GlobalEventManager.CommonAssets.wispSoulMasterPrefabMasterComponent))))
            {
                c.EmitDelegate((CharacterMaster soulWispOriginalMasterPrefab) =>
                {
                    if (ExplicitSpawnRandomizerController.TryGetReplacementMaster(soulWispOriginalMasterPrefab, out CharacterMaster replacementPrefab))
                    {
                        return replacementPrefab;
                    }
                    else
                    {
                        return soulWispOriginalMasterPrefab;
                    }
                });
            }

            // Healing Core
            c.Index = 0;
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(GlobalEventManager.CommonAssets), nameof(GlobalEventManager.CommonAssets.eliteEarthHealerMaster))))
            {
                c.EmitDelegate<Func<GameObject, GameObject>>(ExplicitSpawnRandomizerController.GetSummonReplacement);
            }

            // Void Infestor (From killing void elite)
            c.Index = 0;
            if (c.TryFindNext(out findCursors,
                              x => x.MatchLdstr("RoR2/DLC1/EliteVoid/VoidInfestorMaster.prefab"),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => Addressables.LoadAssetAsync<GameObject>(default(object)))),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<AsyncOperationHandle<GameObject>>(_ => _.WaitForCompletion()))))
            {
                ILCursor last = findCursors[findCursors.Length - 1];

                last.Index++;

                last.EmitDelegate<Func<GameObject, GameObject>>(ExplicitSpawnRandomizerController.GetSummonReplacement);
            }

            c.Index = 0;
            while (c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => NetworkServer.Spawn(default)))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((GameObject instantiated) =>
                {
                    if (NetworkServer.active && instantiated.GetComponent<CharacterMaster>())
                    {
                        ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(instantiated);
                    }
                });

                c.Index++;
            }

            c.Index = 0;
            while (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<MasterSummon>(nameof(MasterSummon.Perform))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((CharacterMaster master) =>
                {
                    if (NetworkServer.active)
                    {
                        ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(master.gameObject);
                    }
                });
            }
        }
    }
}
