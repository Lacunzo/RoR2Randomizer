using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : MonoBehaviour
    {
        protected CharacterMaster _master;
        protected CharacterBody _cachedBody;

        protected abstract BossReplacementType ReplacementType { get; }

        public void Initialize()
        {
            _master = GetComponent<CharacterMaster>();

            if (NetworkServer.active)
            {
                StartCoroutine(initializeServer());
            }

            StartCoroutine(initializeClient());
        }

        protected IEnumerator getBody(CoroutineOut<CharacterBody> body)
        {
            if (_cachedBody)
            {
                body.Result = _cachedBody;
                yield break;
            }

            body.Result = _master.GetBody();
            if (!body.Result)
            {
                while (_master && !_master.hasBody)
                {
                    yield return 0;
                }

                if (!_master)
                    yield break;

                body.Result = _master.GetBody();
            }

            _cachedBody = body.Result;
        }

        protected virtual IEnumerator initializeClient()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeClient)}");
#endif
            yield break;
        }

        protected virtual IEnumerator initializeServer()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeServer)}");
#endif

            new SyncBossReplacementCharacter(_master.gameObject, ReplacementType).Send(NetworkDestination.Clients);

#if DEBUG
            Log.Debug($"Sent {nameof(SyncBossReplacementCharacter)} to clients");
#endif

            CharacterBody body = _cachedBody;
            if (!body)
            {
                CoroutineOut<CharacterBody> bodyOut = new CoroutineOut<CharacterBody>();
                yield return getBody(bodyOut);

                body = bodyOut.Result;
            }

            if (body)
            {
                if (body.bodyIndex == BodyCatalog.FindBodyIndex("EquipmentDroneBody"))
                {
                    Inventory inventory = _master.inventory;
                    if (inventory && inventory.GetEquipmentIndex() == EquipmentIndex.None)
                    {
                        EquipmentIndex equipment = BossRandomizerController.AvailableDroneEquipments.Get.GetRandomOrDefault(EquipmentIndex.None);
                        inventory.SetEquipmentIndex(equipment);

#if DEBUG
                        Log.Debug($"Gave {Language.GetString(EquipmentCatalog.GetEquipmentDef(equipment).nameToken)} to {Language.GetString(body.baseNameToken)}");
#endif
                    }
                }
                else if (body.bodyIndex == BodyCatalog.FindBodyIndex("DroneCommanderBody")) // Col. Droneman
                {
                    Inventory inventory = _master.inventory;
                    if (inventory)
                    {
                        const int NUM_DRONE_PARTS = 1;

                        Patches.Reverse.DroneWeaponsBehavior.SetNumDroneParts(inventory, NUM_DRONE_PARTS);

#if DEBUG
                        Log.Debug($"Gave {NUM_DRONE_PARTS} drone parts to {Language.GetString(body.baseNameToken)}");
#endif
                    }
                }
            }
        }
    }
}
