﻿using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : MonoBehaviour
    {
        protected CharacterMaster _master;
        protected CharacterBody _body;

        protected abstract BossReplacementType replacementType { get; }

        protected abstract CharacterMaster originalBossMasterPrefab { get; }
        
        protected CharacterBody originalBossBodyPrefab
        {
            get
            {
                CharacterMaster originalMasterPrefab = originalBossMasterPrefab;
                if (originalMasterPrefab && originalMasterPrefab.bodyPrefab && originalMasterPrefab.bodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body))
                {
                    return body;
                }

                return null;
            }
        }

        protected virtual bool replaceBossDropEvenIfExisting => false;

        public void Initialize()
        {
            _master = GetComponent<CharacterMaster>();

            StartCoroutine(waitForBodyInitialized());

            if (NetworkServer.active)
            {
                initializeServer();
            }

            initializeClient();
        }

        IEnumerator waitForBodyInitialized()
        {
            if (!_body)
            {
                _body = _master.GetBody();
                if (!_body)
                {
                    while (_master && !_master.hasBody)
                    {
                        yield return 0;
                    }

                    if (!_master)
                        yield break;

                    _body = _master.GetBody();
                }
            }

            if (_body)
            {
                bodyResolved();
            }
        }

        protected virtual void bodyResolved()
        {
            Inventory inventory = _master.inventory;
            if (inventory)
            {
                GivePickupsOnStart givePickupsOnStart = _master.GetComponent<GivePickupsOnStart>();

                if (originalBossMasterPrefab)
                {
                    GivePickupsOnStart originalGivePickupsOnStart = originalBossMasterPrefab.GetComponent<GivePickupsOnStart>();

                    ItemIndex adaptiveArmorItemIndex = RoR2Content.Items.AdaptiveArmor.itemIndex;
                    if (originalGivePickupsOnStart && originalGivePickupsOnStart.HasItems(adaptiveArmorItemIndex, out int originalAdaptiveArmorCount))
                    {
                        inventory.GiveItem(adaptiveArmorItemIndex, originalAdaptiveArmorCount);
                    }
                }

                if (_body.bodyIndex == BodyCatalog.FindBodyIndex("EquipmentDroneBody"))
                {
                    if ((!givePickupsOnStart || !givePickupsOnStart.HasAnyEquipment()) && inventory.GetEquipmentIndex() == EquipmentIndex.None)
                    {
                        EquipmentIndex equipment = CharacterReplacements.AvailableDroneEquipments.Get.GetRandomOrDefault(EquipmentIndex.None);
                        inventory.SetEquipmentIndex(equipment);

#if DEBUG
                        Log.Debug($"Gave {Language.GetString(EquipmentCatalog.GetEquipmentDef(equipment).nameToken)} to {Language.GetString(_body.baseNameToken)}");
#endif
                    }
                }
                else if (_body.bodyIndex == BodyCatalog.FindBodyIndex("DroneCommanderBody")) // Col. Droneman
                {
                    const int NUM_DRONE_PARTS = 1;

                    Patches.Reverse.DroneWeaponsBehavior.SetNumDroneParts(inventory, NUM_DRONE_PARTS);

#if DEBUG
                    Log.Debug($"Gave {NUM_DRONE_PARTS} drone parts to {Language.GetString(_body.baseNameToken)}");
#endif
                }
            }

            CharacterBody originalBodyprefab = originalBossBodyPrefab;
            if (originalBodyprefab)
            {
                if (originalBodyprefab.TryGetComponent<DeathRewards>(out DeathRewards prefabDeathRewards) && prefabDeathRewards.bossDropTable)
                {
                    DeathRewards deathRewards = _body.gameObject.GetOrAddComponent<DeathRewards>();
                    if (!deathRewards.bossDropTable || deathRewards.bossDropTable == null || replaceBossDropEvenIfExisting)
                    {
                        deathRewards.bossDropTable = prefabDeathRewards.bossDropTable;
                    }
                }

                HealthComponent healthComponent = _body.healthComponent;
                if (healthComponent && originalBodyprefab.TryGetComponent<HealthComponent>(out HealthComponent prefabHealthComponent))
                {
#if DEBUG
                    float oldValue = healthComponent.globalDeathEventChanceCoefficient;
#endif

                    healthComponent.globalDeathEventChanceCoefficient = prefabHealthComponent.globalDeathEventChanceCoefficient;

#if DEBUG
                    if (oldValue != healthComponent.globalDeathEventChanceCoefficient)
                    {
                        Log.Debug($"{nameof(BaseBossReplacement)}: overriding {nameof(HealthComponent.globalDeathEventChanceCoefficient)} for {replacementType} replacement {_master.name} ({oldValue} -> {healthComponent.globalDeathEventChanceCoefficient})");
                    }
#endif
                }
            }
        }

        protected virtual void initializeClient()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeClient)}");
#endif
        }

        protected virtual void initializeServer()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeServer)}");
#endif

            new SyncBossReplacementCharacter(_master.gameObject, replacementType).Send(NetworkDestination.Clients);

#if DEBUG
            Log.Debug($"Sent {nameof(SyncBossReplacementCharacter)} to clients");
#endif
        }

        protected void setBodySubtitle(string subtitleToken)
        {
            if (_body && _body.subtitleNameToken != subtitleToken)
            {
                _body.subtitleNameToken = subtitleToken;

                // Update BossGroup
                if (_master.isBoss)
                {
                    resetBossGroupSubtitle();
                }
            }
        }

        void resetBossGroupSubtitle()
        {
            BossGroup[] bossGroups = GameObject.FindObjectsOfType<BossGroup>();
            foreach (BossGroup group in bossGroups)
            {
                for (int i = 0; i < group.bossMemoryCount; i++)
                {
                    if (group.bossMemories[i].cachedMaster == _master)
                    {
                        // Force a refresh of the boss subtitle
                        group.bestObservedName = string.Empty;
                        group.bestObservedSubtitle = string.Empty;
                        return;
                    }
                }
            }
        }
    }
}
