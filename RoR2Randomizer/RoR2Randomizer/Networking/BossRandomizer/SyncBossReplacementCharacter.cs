using R2API.Networking.Interfaces;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.BossRandomizer
{
    public class SyncBossReplacementCharacter : INetMessage
    {
        public delegate void OnReceivedDelegate(GameObject masterObject, BossReplacementType replacementType);
        public static event OnReceivedDelegate OnReceive;

        NetworkInstanceId _masterObjectId;
        BossReplacementType _replacementType;

        public SyncBossReplacementCharacter()
        {
        }

        public SyncBossReplacementCharacter(GameObject masterObject, BossReplacementType replacementType)
        {
            _masterObjectId = masterObject.GetComponent<NetworkIdentity>().netId;
            _replacementType = replacementType;

            if (!replacementType.IsValid())
            {
                Log.Warning($"{nameof(SyncBossReplacementCharacter)} is about to sync a boss replacement with an invalid boss type of {replacementType}! {nameof(masterObject)}: {masterObject}");
            }
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.Write(_masterObjectId);
            writer.WritePackedUInt32((uint)_replacementType);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _masterObjectId = reader.ReadNetworkId();
            _replacementType = (BossReplacementType)reader.ReadPackedUInt32();
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncBossReplacementCharacter)}.{nameof(INetMessage.OnReceived)}(): _masterObjectId: {_masterObjectId}, _replacementType: {_replacementType}");
#endif

            if (!_replacementType.IsValid())
            {
                Log.Warning($"{nameof(SyncBossReplacementCharacter)} received a boss replacement with invalid replacement type {_replacementType}! {nameof(_masterObjectId)}: {_masterObjectId}");
            }

            if (_masterObjectId.IsEmpty())
            {
                Log.Warning("Recieved empty master object id from server, aborting");
                return;
            }

            if (!NetworkServer.active)
            {
#if DEBUG
                Log.Debug($"Recieved {nameof(SyncBossReplacementCharacter)} as non-server");
#endif
                IEnumerator waitThenSendEvent()
                {
#if DEBUG
                    float timeStarted = Time.unscaledTime;
#endif

                    GameObject obj;
                    while (!(obj = ClientScene.FindLocalObject(_masterObjectId)))
                    {
                        yield return 0;
                    }

#if DEBUG
                    Log.Debug($"Waited {Time.unscaledTime - timeStarted:F2} seconds for client object");
#endif

                    OnReceive?.Invoke(obj, _replacementType);
                }

                Main.Instance.StartCoroutine(waitThenSendEvent());
            }
#if DEBUG
            else
            {
                Log.Debug($"Recieved {nameof(SyncBossReplacementCharacter)} as server, skipping");
            }
#endif
        }
    }
}
