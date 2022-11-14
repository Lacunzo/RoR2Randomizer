using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Utility;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.CharacterReplacements
{
    public sealed class SyncCharacterMasterReplacements : NetworkMessageBase
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

        public override void Serialize(NetworkWriter writer)
        {
            _masterReplacements.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _masterReplacements = IndexReplacementsCollection.Deserialize(reader);
        }

        public override void OnReceived()
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
