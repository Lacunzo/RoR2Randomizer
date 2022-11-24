using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Aurelionite;
using RoR2Randomizer.Utility;
using System;
using UnityEngine;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using R2API;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    public partial class BossRandomizerController
    {
        public static class Aurelionite
        {
            static bool IsEnabled => _instance && _instance.IsRandomizerEnabled && (ConfigManager.BossRandomizer.RandomizeAurelionite || CharacterReplacements.IsAnyForcedCharacterModeEnabled);

            const string MASTER_NAME = "TitanGoldMaster";

            static readonly RunSpecific<GameObject> _aurelioniteMasterReplacementPrefab = new RunSpecific<GameObject>((out GameObject overridePrefab) =>
            {
                if (IsEnabled)
                {
                    overridePrefab = CharacterReplacements.GetReplacementMasterPrefab(MASTER_NAME);
                    if (overridePrefab &&
                        overridePrefab.TryGetComponent<CharacterMaster>(out CharacterMaster master) &&
                        master.bodyPrefab && master.bodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body))
                    {
                        replaceAurelioniteInItemPickupText(RoR2Content.Items.TitanGoldDuringTP, body.GetDisplayName());
                        return true;
                    }
                }

                overridePrefab = null;
                return false;
            });

            static LanguageAPI.LanguageOverlay _aurelionitePickupOverlay;

            static void replaceAurelioniteInItemPickupText(ItemDef item, string replacementName)
            {
                _aurelionitePickupOverlay?.Remove();

                string itemToken = item.pickupToken;
                string itemPickupDisplay = Language.GetString(itemToken);

                string aurelioniteBodyName = Language.GetString("TITANGOLD_BODY_NAME");
                _aurelionitePickupOverlay = LanguageAPI.AddOverlay(itemToken, itemPickupDisplay.Replace(aurelioniteBodyName, replacementName));
            }

            public static bool TryGetAurelioniteMasterReplacementPrefab(out GameObject replacementPrefab)
            {
                if (IsEnabled)
                {
#if DEBUG
                    if (CharacterReplacements.DebugMode != DebugMode.None)
                    {
                        replacementPrefab = CharacterReplacements.GetReplacementMasterPrefab(MASTER_NAME);
                    }
                    else
#endif
                    {
                        replacementPrefab = _aurelioniteMasterReplacementPrefab;
                    }

                    return (bool)replacementPrefab;
                }

                replacementPrefab = null;
                return false;
            }

            public static bool TryGetAurelioniteMasterReplacementBodyIndex(out BodyIndex replacementIndex)
            {
                if (TryGetAurelioniteMasterReplacementPrefab(out GameObject replacementPrefab) &&
                    replacementPrefab.TryGetComponent<CharacterMaster>(out CharacterMaster master) &&
                    master.bodyPrefab && master.bodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body))
                {
                    replacementIndex = body.bodyIndex;
                    return true;
                }

                replacementIndex = BodyIndex.None;
                return false;
            }

            public static event Action<AurelioniteReplacement> AurelioniteReplacementReceivedClient;

            public static void Initialize()
            {
                if (AurelioniteFightTracker.Instance != null)
                {
                    AurelioniteFightTracker.Instance.IsInFight.OnChanged += IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;

                Run.onRunDestroyGlobal += runEnd;
            }

            public static void Uninitialize()
            {
                if (AurelioniteFightTracker.Instance != null)
                {
                    AurelioniteFightTracker.Instance.IsInFight.OnChanged -= IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;

                Run.onRunDestroyGlobal -= runEnd;

                _aurelioniteMasterReplacementPrefab.Dispose();
            }

            static void runEnd(Run instance)
            {
                _aurelionitePickupOverlay?.Remove();
                _aurelionitePickupOverlay = null;
            }

            static void IsInFight_OnChanged(bool isInFight)
            {
                if (IsEnabled)
                {
                    if (isInFight)
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (ref SpawnCard card, out GenericScriptedSpawnHook.ResetCardDelegate resetCardFunc) =>
                        {
                            if (card == SpawnCardTracker.AurelioniteSpawnCard && TryGetAurelioniteMasterReplacementPrefab(out GameObject overridePrefab))
                            {
                                GameObject originalPrefab = card.prefab;
                                resetCardFunc = (ref SpawnCard c) => c.prefab = originalPrefab;

                                card.prefab = overridePrefab;
                            }
                            else
                            {
                                resetCardFunc = null;
                            }
                        };

                        GenericScriptedSpawnHook.OnSpawned += handleSpawnedAurelioniteCharacterServer;
                    }
                    else
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;
                        GenericScriptedSpawnHook.OnSpawned -= handleSpawnedAurelioniteCharacterServer;
                    }
                }
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType, MasterCatalog.MasterIndex? originalMasterIndex)
            {
                switch (replacementType)
                {
                    case BossReplacementType.Aurelionite:
#if DEBUG
                        Log.Debug($"Running {nameof(handleSpawnedAurelioniteCharacterClient)}");
#endif
                        handleSpawnedAurelioniteCharacterClient(masterObject);
                        break;
                }
            }

            static void handleSpawnedAurelioniteCharacterClient(GameObject masterObject)
            {
                AurelioniteReplacement aurelioniteReplacement = masterObject.AddComponent<AurelioniteReplacement>();
#if DEBUG
                Log.Debug($"Adding {nameof(AurelioniteReplacement)} component to {masterObject}");
#endif
                aurelioniteReplacement.Initialize();

                AurelioniteReplacementReceivedClient?.Invoke(aurelioniteReplacement);
            }

            static void handleSpawnedAurelioniteCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (IsEnabled && 
                    AurelioniteFightTracker.Instance != null && AurelioniteFightTracker.Instance.IsInFight && 
                    spawnResult.spawnRequest.spawnCard == SpawnCardTracker.AurelioniteSpawnCard)
                {
                    AurelioniteReplacement aurelioniteReplacement = spawnResult.spawnedInstance.AddComponent<AurelioniteReplacement>();
#if DEBUG
                    Log.Debug($"Adding {nameof(AurelioniteReplacement)} component to {spawnResult.spawnedInstance}");
#endif
                    aurelioniteReplacement.Initialize();
                }
            }
        }
    }
}
