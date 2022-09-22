using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public partial class BossRandomizerController : Singleton<BossRandomizerController>
    {
        public static class Mithrix
        {
            public static readonly SerializableEntityStateType MithrixHurtInitialState = new SerializableEntityStateType(typeof(SpellChannelEnterState));

            public static readonly InitializeOnAccess<ReturnStolenItemsOnGettingHit> MithrixReturnItemsComponent = new InitializeOnAccess<ReturnStolenItemsOnGettingHit>(() =>
            {
                return Caches.MasterPrefabs["BrotherHurtMaster"].bodyPrefab.GetComponent<ReturnStolenItemsOnGettingHit>();
            });

            public static bool TryGetOverridePrefabFor(SpawnCard card, out GameObject overridePrefab)
            {
                if (ConfigManager.BossRandomizer.Enabled && MithrixPhaseTracker.IsInMithrixFight)
                {
                    if ((ConfigManager.BossRandomizer.RandomizeMithrix && (card == SpawnCardTracker.MithrixNormalSpawnCard || card == SpawnCardTracker.MithrixHurtSpawnCard))
                     || (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(card)))
                    {
                        overridePrefab = getBossOverrideMasterPrefab();

#if DEBUG
                        Log.Debug($"MithrixRandomizer: Replaced {card.prefab} with {overridePrefab}");
#endif

                        return (bool)overridePrefab;
                    }
                }

                overridePrefab = null;
                return false;
            }

            public static void HandleSpawnedMithrixCharacterClient(GameObject masterObject, BossReplacementType type)
            {
                BaseMithrixReplacement baseMithrixReplacement;
                if (type == BossReplacementType.MithrixPhase2)
                {
#if DEBUG
                    Log.Debug($"Adding {nameof(MithrixPhase2EnemiesReplacement)} component to {masterObject}");
#endif
                    baseMithrixReplacement = masterObject.AddComponent<MithrixPhase2EnemiesReplacement>();
                }
                else
                {
                    bool isHurt;
                    if ((isHurt = type == BossReplacementType.MithrixHurt) || type == BossReplacementType.MithrixNormal)
                    {
#if DEBUG
                        Log.Debug($"Adding {nameof(MainMithrixReplacement)} component to {masterObject}, isHurt={isHurt}");
#endif

                        MainMithrixReplacement mainMithrixReplacement;
                        baseMithrixReplacement = mainMithrixReplacement = masterObject.AddComponent<MainMithrixReplacement>();
                        mainMithrixReplacement.IsHurt = isHurt;
                    }
                    else
                    {
                        Log.Warning($"Invalid mithrix replacement type '{type}'");
                        return;
                    }
                }

                baseMithrixReplacement.Initialize();
            }

            public static void HandleSpawnedMithrixCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && MithrixPhaseTracker.IsInMithrixFight)
                {
                    BaseMithrixReplacement baseMithrixReplacement = null;
                    if (MithrixPhaseTracker.Phase == 2)
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(spawnResult.spawnRequest.spawnCard))
                        {
                            baseMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MithrixPhase2EnemiesReplacement>();
                        }
                    }
                    else
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrix)
                        {
                            bool isHurtMithrixReplacement;
                            if ((isHurtMithrixReplacement = spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixHurtSpawnCard) ||
                                spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixNormalSpawnCard)
                            {
                                MainMithrixReplacement mainMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MainMithrixReplacement>();
                                mainMithrixReplacement.IsHurt = isHurtMithrixReplacement;

                                baseMithrixReplacement = mainMithrixReplacement;
                            }
                        }
                    }

                    if (baseMithrixReplacement)
                    {
                        baseMithrixReplacement.Initialize();
                    }
                }
            }

            public static bool IsReplacedMithrix(GameObject master)
            {
                return master.GetComponent<MainMithrixReplacement>();
            }

            public static bool IsReplacedMithrixPhase2Spawn(GameObject master)
            {
                return master.GetComponent<MithrixPhase2EnemiesReplacement>();
            }

            public static bool IsReplacedPartOfMithrixFight(GameObject master)
            {
                return IsReplacedMithrix(master) || IsReplacedMithrixPhase2Spawn(master);
            }
        }
    }
}
