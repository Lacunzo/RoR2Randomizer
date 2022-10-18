using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.CharacterReplacements
{
    public sealed class SyncCharacterMasterReplacements : INetMessage
    {
        public delegate void OnReceivedDelegate(IndexReplacementsCollection masterReplacements);
        public static event OnReceivedDelegate OnReceive;

        IndexReplacementsCollection _masterReplacements;

        public SyncCharacterMasterReplacements()
        {
        }

        public SyncCharacterMasterReplacements(IndexReplacementsCollection masterReplacements)
        {
            _masterReplacements = masterReplacements;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            _masterReplacements.Serialize(writer);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _masterReplacements = IndexReplacementsCollection.Deserialize(reader);
        }

        void INetMessage.OnReceived()
        {
            if (NetworkServer.active)
            {
#if DEBUG
                Log.Debug($"Received {nameof(SyncCharacterMasterReplacements)} as server, skipping");
#endif
            }
            else if (NetworkClient.active)
            {
#if DEBUG
                Log.Debug($"Received {nameof(SyncCharacterMasterReplacements)} as client, applying replacements");
#endif

                OnReceive?.Invoke(_masterReplacements);
            }
        }
    }
}
