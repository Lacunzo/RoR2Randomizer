using HG;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        void Awake()
        {
            _group = GetComponent<HurtBoxGroup>();

            NetworkingManager.RegisterMessageProvider(this);
        }

        void Start()
        {
            const string LOG_PREFIX = $"{nameof(HurtBoxGroupRandomizerData)}.{nameof(Start)} ";

            if (NetworkServer.active)
            {
                if (!NetworkServer.dontListen)
                {
                    this.TrySendAll(NetworkDestination.Clients);
                }
            }
            else if (NetworkClient.active)
            {
                if (!_group || _group.hurtBoxes == null)
                {
                    Log.Warning(LOG_PREFIX + $"invalid {nameof(_group)} reference");
                }
                else
                {
                    if (OriginalIsSniperTargetValues.Length != _group.hurtBoxes.Length)
                    {
                        Log.Warning(LOG_PREFIX + $"mismatched group sizes! {nameof(OriginalIsSniperTargetValues)}.Length={OriginalIsSniperTargetValues.Length} {nameof(_group)}.hurtBoxes.Length={_group.hurtBoxes.Length}");
                    }

                    for (int i = 0; i < _group.hurtBoxes.Length; i++)
                    {
                        HurtBox hurtBox = _group.hurtBoxes[i];
                        if (hurtBox.isSniperTarget != ArrayUtils.GetSafe(OriginalIsSniperTargetValues, i, false))
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
                }
            }
        }

        void OnDestroy()
        {
            NetworkingManager.UnregisterMessageProvider(this);
        }

        public bool SendMessages => OwnerBody && _group;

        public IEnumerable<NetworkMessageBase> GetNetMessages()
        {
            const string LOG_PREFIX = $"{nameof(HurtBoxGroupRandomizerData)}.{nameof(GetNetMessages)} ";

            if (OwnerBody && _group)
            {
                if (OriginalIsSniperTargetValues.Length != _group.hurtBoxes.Length)
                {
                    Log.Warning(LOG_PREFIX + $"mismatched group sizes! {nameof(OriginalIsSniperTargetValues)}.Length={OriginalIsSniperTargetValues.Length} {nameof(_group)}.hurtBoxes.Length={_group.hurtBoxes.Length}");
                }

                yield return new SyncSniperWeakPointReplacements(OwnerBody, _group.hurtBoxes.Where((h, i) => h.isSniperTarget != ArrayUtils.GetSafe(OriginalIsSniperTargetValues, i, false)), _group.hurtBoxes.Length);
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"{nameof(OwnerBody)} or {_group} is null or destroyed!");
            }
        }
    }
}
