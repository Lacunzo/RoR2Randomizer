using RoR2;
using RoR2Randomizer.Configuration;
using UnityEngine;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Networking.BossRandomizer;
using System;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    public partial class BossRandomizerController
    {
        public static class HoldoutBoss
        {
            static bool IsEnabled => _instance && _instance.IsRandomizerEnabled && (ConfigManager.BossRandomizer.RandomizeTeleporterBosses || CharacterReplacements.IsAnyForcedCharacterModeEnabled);

            public static void Initialize()
            {
                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;
            }

            public static void Uninitialize()
            {
                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType, MasterCatalog.MasterIndex? originalMasterIndex)
            {
                switch (replacementType)
                {
                    case BossReplacementType.TeleporterBoss:
                        handleSpawnedTeleporterBossReplacement(masterObject, originalMasterIndex.GetValueOrDefault(MasterCatalog.MasterIndex.none));
                        break;
                }
            }

            public static void TryReplaceDirectorSpawnRequest(DirectorSpawnRequest spawnRequest)
            {
                if (IsEnabled)
                {
                    CharacterReplacements.TryReplaceDirectorSpawnRequest(spawnRequest, handleSpawnedTeleporterBossReplacement);
                }
            }

            static void handleSpawnedTeleporterBossReplacement(GameObject masterObject, MasterCatalog.MasterIndex originalMasterIndex)
            {
                if (masterObject)
                {
                    TeleporterBossReplacement teleporterBossReplacement = masterObject.AddComponent<TeleporterBossReplacement>();
                    teleporterBossReplacement.SetOriginalMasterIndex(originalMasterIndex);
                    teleporterBossReplacement.Initialize();
                }
            }
        }
    }
}
