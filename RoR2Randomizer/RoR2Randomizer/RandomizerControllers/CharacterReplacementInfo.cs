using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers
{
    public abstract class CharacterReplacementInfo : MonoBehaviour, INetMessageProvider
    {
        bool _initializeOnEnable;

        protected CharacterMaster _master;
        protected CharacterBody _body;

        protected abstract CharacterMaster originalMasterPrefab { get; }

        protected CharacterBody originalBodyPrefab
        {
            get
            {
                CharacterMaster originalMaster = originalMasterPrefab;
                if (originalMaster && originalMaster.bodyPrefab && originalMaster.bodyPrefab.TryGetComponent(out CharacterBody body))
                {
                    return body;
                }

                return null;
            }
        }

        protected abstract bool isNetworked { get; }
        bool INetMessageProvider.SendMessages => isNetworked;

        void Awake()
        {
            _master = GetComponent<CharacterMaster>();

            if (isNetworked)
            {
                NetworkingManager.RegisterMessageProvider(this);
            }
        }

        void OnDestroy()
        {
            if (isNetworked)
            {
                NetworkingManager.UnregisterMessageProvider(this);
            }
        }

        void Start()
        {
            if (_initializeOnEnable)
            {
                _initializeOnEnable = false;
                Initialize();
            }
        }

        public void Initialize()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(waitForBodyInitialized());

                if (NetworkServer.active)
                {
                    initializeServer();
                }

                initializeClient();
            }
            else
            {
                _initializeOnEnable = true;
            }
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

                if (originalMasterPrefab)
                {
                    GivePickupsOnStart originalGivePickupsOnStart = originalMasterPrefab.GetComponent<GivePickupsOnStart>();

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
                        EquipmentIndex equipment = CharacterReplacements.AvailableDroneEquipments.GetRandomOrDefault(EquipmentIndex.None);
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
        }

        protected virtual void initializeClient()
        {
        }

        protected virtual void initializeServer()
        {
            if (isNetworked && !NetworkServer.dontListen)
            {
                foreach (NetworkMessageBase message in getNetMessages())
                {
                    message.SendTo(NetworkDestination.Clients);
                }
            }
        }

        protected virtual IEnumerable<NetworkMessageBase> getNetMessages()
        {
            yield break;
        }
        IEnumerable<NetworkMessageBase> INetMessageProvider.GetNetMessages()
        {
            return getNetMessages();
        }
    }
}
