using R2API.Networking.Interfaces;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.Boss;
using System;
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

        GameObject _masterObject;
        BossReplacementType _replacementType;

        public SyncBossReplacementCharacter()
        {
        }

        public SyncBossReplacementCharacter(GameObject masterObject, BossReplacementType replacementType)
        {
            _masterObject = masterObject;
            _replacementType = replacementType;

            if (!replacementType.IsValid())
            {
                Log.Warning($"{nameof(SyncBossReplacementCharacter)} is about to sync a boss replacement with an invalid boss type of {replacementType}! {nameof(masterObject)}: {masterObject}");
            }
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.Write(_masterObject);
            writer.WritePackedUInt32((uint)_replacementType);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _masterObject = reader.ReadGameObject();
            _replacementType = (BossReplacementType)reader.ReadPackedUInt32();
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncBossReplacementCharacter)}.{nameof(INetMessage.OnReceived)}(): _masterObject: {_masterObject}, _replacementType: {_replacementType}");
#endif

            if (!_replacementType.IsValid())
            {
                Log.Warning($"{nameof(SyncBossReplacementCharacter)} received a boss replacement with invalid replacement type {_replacementType}! {nameof(_masterObject)}: {_masterObject}");
            }

            if (!NetworkServer.active)
            {
#if DEBUG
                Log.Debug($"Recieved {nameof(SyncBossReplacementCharacter)} as non-server");
#endif

                OnReceive?.Invoke(_masterObject, _replacementType);
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
