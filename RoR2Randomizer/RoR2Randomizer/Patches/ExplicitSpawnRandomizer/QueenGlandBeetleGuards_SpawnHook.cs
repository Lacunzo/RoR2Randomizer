using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class QueenGlandBeetleGuards_SpawnHook
    {
        static ILHook BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned_ILHook = null;

        static void Apply()
        {
            IL.RoR2.Items.BeetleGlandBodyBehavior.FixedUpdate += BeetleGlandBodyBehavior_FixedUpdate;
        }

        static void Cleanup()
        {
            IL.RoR2.Items.BeetleGlandBodyBehavior.FixedUpdate -= BeetleGlandBodyBehavior_FixedUpdate;

            BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned_ILHook?.Undo();
        }

        static void BeetleGlandBodyBehavior_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((DirectorSpawnRequest directorSpawnRequest) =>
                {
                    if (!ConfigManager.ExplicitSpawnRandomizer.RandomizeQueensGlandBeetleGuards)
                        return;

                    ExplicitSpawnRandomizerController.TryReplaceDirectorSpawnRequest(directorSpawnRequest);

                    if (ExplicitSpawnRandomizerController.IsActive)
                    {
                        if (BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned_ILHook == null)
                        {
                            if (directorSpawnRequest.onSpawnedServer != null)
                            {
                                Delegate[] invokeList = directorSpawnRequest.onSpawnedServer.GetInvocationList();
                                if (invokeList.Length > 0)
                                {
                                    BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned_ILHook = new ILHook(invokeList[0].Method, BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned);

#if DEBUG
                                    Log.Debug("Apply OnGuardMasterSpawned_ILHook");
#endif
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                Log.Warning("failed to find patch location");
            }
        }

        static void BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((SpawnCard.SpawnResult spawnResult) =>
            {
                if (NetworkServer.active && ConfigManager.ExplicitSpawnRandomizer.RandomizeQueensGlandBeetleGuards && spawnResult.success && spawnResult.spawnedInstance)
                {
                    ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(spawnResult.spawnedInstance);
                }
            });

            int patchLocationsFound = 0;
            while (c.TryGotoNext(x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<Component>(_ => _.GetComponent<Deployable>()))))
            {
                patchLocationsFound++;

                c.Emit(OpCodes.Dup);
                c.Index++;
                c.EmitDelegate((Component component, Deployable deployable) =>
                {
                    if (!deployable)
                    {
                        if (ExplicitSpawnRandomizerController.IsActive && ConfigManager.ExplicitSpawnRandomizer.RandomizeQueensGlandBeetleGuards)
                        {
                            deployable = component.gameObject.AddComponent<Deployable>();
                            deployable.onUndeploy = new UnityEvent();
                        }
                    }

                    return deployable;
                });
            }

            if (patchLocationsFound == 0)
            {
                Log.Warning("failed to find any patch locations in BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned");
            }
#if DEBUG
            else
            {
                Log.Debug($"found {patchLocationsFound} patch locations in BeetleGlandBodyBehavior_FixedUpdate_OnGuardMasterSpawned");
            }
#endif
        }
    }
}
