using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer.Director
{
    [PatchClass]
    static class ArenaMissionController_ReplaceMonsterDisplay
    {
        static void Apply()
        {
            IL.RoR2.ArenaMissionController.AddMonsterType += ArenaMissionController_AddMonsterType;
        }

        static void Cleanup()
        {
            IL.RoR2.ArenaMissionController.AddMonsterType -= ArenaMissionController_AddMonsterType;
        }

        static void ArenaMissionController_AddMonsterType(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(ArenaMissionController_ReplaceMonsterDisplay)}.{nameof(ArenaMissionController_AddMonsterType)} ";

            ILCursor c = new ILCursor(il);

            int patchCount = 0;
            while (c.TryGotoNext(MoveType.After,
                                 x => x.MatchLdloc(out _),
                                 x => x.MatchLdfld<DirectorCard>(nameof(DirectorCard.spawnCard)),
                                 x => x.MatchLdfld<SpawnCard>(nameof(SpawnCard.prefab)),
                                 x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<GameObject>(_ => _.GetComponent<CharacterMaster>()))))
            {
                c.EmitDelegate(static (CharacterMaster masterPrefab) =>
                {
                    if (ExplicitSpawnRandomizerController.IsActive && ConfigManager.ExplicitSpawnRandomizer.RandomizeDirectorSpawns)
                    {
                        return ExplicitSpawnRandomizerController.GetSummonReplacement(masterPrefab);
                    }

                    return masterPrefab;
                });

                patchCount++;
            }

            if (patchCount == 0)
            {
                Log.Warning(LOG_PREFIX + "no patch location found");
            }
#if DEBUG
            else
            {
                Log.Debug(LOG_PREFIX + $"patched {patchCount} locations");
            }
#endif
        }
    }
}
