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
        public delegate void OnReceivedDelegate(ReplacementDictionary<int> masterReplacements);
        public static event OnReceivedDelegate OnReceive;

        ReplacementDictionary<int> _masterReplacements;

        public SyncCharacterMasterReplacements()
        {
        }

        public SyncCharacterMasterReplacements(ReplacementDictionary<int> masterReplacements)
        {
            _masterReplacements = masterReplacements;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_masterReplacements.Count);
            foreach (KeyValuePair<int, int> pair in _masterReplacements)
            {
                writer.WritePackedIndex32(pair.Key);
                writer.WritePackedIndex32(pair.Value);
            }
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            Dictionary<int, int> masterReplacements = new Dictionary<int, int>();

            uint count = reader.ReadPackedUInt32();
            for (uint i = 0; i < count; i++)
            {
                masterReplacements.Add(reader.ReadPackedIndex32(), reader.ReadPackedIndex32());
            }

            _masterReplacements = new ReplacementDictionary<int>(masterReplacements);
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
