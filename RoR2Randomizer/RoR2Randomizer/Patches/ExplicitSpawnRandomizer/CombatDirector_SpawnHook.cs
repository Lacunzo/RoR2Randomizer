using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.Boss;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class CombatDirector_SpawnHook
    {
        static void Apply()
        {
            IL.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
        }

        static void Cleanup()
        {
            IL.RoR2.CombatDirector.Spawn -= CombatDirector_Spawn;
        }

        static void CombatDirector_Spawn(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(static (DirectorSpawnRequest spawnRequest, CombatDirector instance) =>
                {
#if DEBUG
                    Log.Debug($"Attempting spawn replacement from {nameof(CombatDirector)} {instance} ({instance.customName})");
#endif

                    if (!ConfigManager.ExplicitSpawnRandomizer.RandomizeDirectorSpawns && instance.GetComponent<DirectorCore>())
                    {
#if DEBUG
                        Log.Debug($"Not replacing spawn from {nameof(CombatDirector)} {instance} due to randomizing director spawns disabled");
#endif

                        return;
                    }

                    bool isHoldoutZone = instance.GetComponent<HoldoutZoneController>();
                    if (isHoldoutZone && string.Equals(instance.customName, "Boss", StringComparison.OrdinalIgnoreCase))
                    {
                        BossRandomizerController.HoldoutBoss.TryReplaceDirectorSpawnRequest(spawnRequest);
                    }
                    else
                    {
                        if (!ConfigManager.ExplicitSpawnRandomizer.RandomizeDirectorSpawns)
                        {
                            // Count all non-boss combat directors on a holdout zone as ordinary director spawns
                            if (isHoldoutZone)
                            {
#if DEBUG
                                Log.Debug($"Not replacing spawn from {nameof(CombatDirector)} {instance} due to randomizing director spawns disabled");
#endif

                                return;
                            }

                            // Void Seed Combat directors don't have customName set, so we have to compare it by object name
                            if (instance.GetComponent<CampDirector>() && instance.name == "Camp 2 - Flavor Props & Void Elites")
                            {
#if DEBUG
                                Log.Debug($"Not replacing spawn from {nameof(CombatDirector)} {instance} due to randomizing director spawns disabled");
#endif

                                return;
                            }
                        }

                        ExplicitSpawnRandomizerController.TryReplaceDirectorSpawnRequest(spawnRequest);
                    }
                });
            }
        }
    }
}
