using HG;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.SniperWeakPoint
{
    [RequireComponent(typeof(HurtBoxGroup))]
    public sealed class HurtBoxGroupRandomizerData : MonoBehaviour, INetMessageProvider
    {
        public CharacterBody OwnerBody;
        public bool[] OriginalIsSniperTargetValues;

        HurtBoxGroup _group;
        bool _isInitialized;

        void Awake()
        {
            _group = GetComponent<HurtBoxGroup>();

            if (TryGetComponent(out CharacterModel characterModel))
            {
                OwnerBody = characterModel.body;
            }
            else
            {
                Log.Warning("could not find owner body");
            }
        }

        void OnEnable()
        {
            NetworkingManager.RegisterMessageProvider(this);

            if (_isInitialized)
            {
                sendToClientsOrSetupHurtboxes();
            }
        }

        void OnDisable()
        {
            NetworkingManager.UnregisterMessageProvider(this);
        }

        public void Initialize(bool[] originalIsSniperTargetValues, bool?[] overrideIsSniperTargetValues)
        {
            if (_isInitialized)
            {
#if DEBUG
                Log.Debug("already initialized!");
#endif
                return;
            }

            if (!_group || _group.hurtBoxes == null)
            {
                Log.Warning("unable to initialize: invalid group reference");
                return;
            }

            OriginalIsSniperTargetValues = originalIsSniperTargetValues;

            for (int i = 0; i < _group.hurtBoxes.Length; i++)
            {
                bool? overrideIsSniperTarget = ArrayUtils.GetSafe(overrideIsSniperTargetValues, i);
                if (overrideIsSniperTarget.HasValue)
                {
                    _group.hurtBoxes[i].isSniperTarget = overrideIsSniperTarget.Value;
                }
            }

            _isInitialized = true;
            sendToClientsOrSetupHurtboxes();
        }

        void sendToClientsOrSetupHurtboxes()
        {
            if (NetworkServer.active)
            {
                if (!NetworkServer.dontListen)
                {
                    this.TrySendAll(NetworkDestination.Clients);
                }
            }
            else if (NetworkClient.active)
            {
                setupHurtboxesClient();
            }
        }

        void setupHurtboxesClient()
        {
            if (!_group || _group.hurtBoxes == null)
            {
                Log.Warning($"invalid {nameof(_group)} reference");
                return;
            }

            if (OriginalIsSniperTargetValues.Length != _group.hurtBoxes.Length)
            {
                Log.Warning($"mismatched group sizes! {nameof(OriginalIsSniperTargetValues)}.Length={OriginalIsSniperTargetValues.Length} {nameof(_group)}.hurtBoxes.Length={_group.hurtBoxes.Length}");
            }

            for (int i = 0; i < _group.hurtBoxes.Length; i++)
            {
                HurtBox hurtBox = _group.hurtBoxes[i];
                if (hurtBox)
                {
                    overrideHurtboxIsSniperTarget(hurtBox, ArrayUtils.GetSafe(OriginalIsSniperTargetValues, i));
                }
            }
        }

        void overrideHurtboxIsSniperTarget(HurtBox hurtBox, bool originalIsSniperTarget)
        {
            if (hurtBox.isSniperTarget != originalIsSniperTarget)
            {
                if (hurtBox.isSniperTarget)
                {
                    if (!hurtBox.isInSniperTargetList)
                    {
                        HurtBox.sniperTargetsList.Add(hurtBox);
                        hurtBox.isInSniperTargetList = true;
                    }
                }
                else
                {
                    if (hurtBox.isInSniperTargetList)
                    {
                        HurtBox.sniperTargetsList.Remove(hurtBox);
                        hurtBox.isInSniperTargetList = false;
                    }
                }
            }
        }

        public bool SendMessages => _isInitialized && OwnerBody && _group;

        public IEnumerable<NetworkMessageBase> GetNetMessages()
        {
            if (OwnerBody && _group)
            {
                if (OriginalIsSniperTargetValues.Length != _group.hurtBoxes.Length)
                {
                    Log.Warning($"mismatched group sizes! {nameof(OriginalIsSniperTargetValues)}.Length={OriginalIsSniperTargetValues.Length} {nameof(_group)}.hurtBoxes.Length={_group.hurtBoxes.Length}");
                }
                
                yield return new SyncSniperWeakPointReplacements(OwnerBody, _group.hurtBoxes.Where((h, i) => h.isSniperTarget != ArrayUtils.GetSafe(OriginalIsSniperTargetValues, i)), _group.hurtBoxes.Length);
            }
            else
            {
                Log.Warning($"{nameof(OwnerBody)} or {_group} is null or destroyed!");
            }
        }
    }
}
