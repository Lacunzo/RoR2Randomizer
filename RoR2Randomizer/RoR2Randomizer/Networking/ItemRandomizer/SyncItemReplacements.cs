using RoR2Randomizer.Networking.Generic.Chunking;
using RoR2Randomizer.Utility;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ItemRandomizer
{
    public class SyncItemReplacements : ChunkedNetworkMessage
    {
        public delegate void OnReceiveDelegate(in IndexReplacementsCollection itemIndexReplacements);
        public static event OnReceiveDelegate OnReceive;

        IndexReplacementsCollection _itemIndexReplacements;

        public SyncItemReplacements(IndexReplacementsCollection itemIndexReplacements)
        {
            _itemIndexReplacements = itemIndexReplacements;
        }

        public SyncItemReplacements()
        {
        }

        public override void Serialize(NetworkWriter writer)
        {
            _itemIndexReplacements.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _itemIndexReplacements = IndexReplacementsCollection.Deserialize(reader);
        }

        public override void OnReceived()
        {
            if (NetworkServer.active)
                return;

#if DEBUG
            Log.Debug("Received item replacements from server");
#endif

            OnReceive?.Invoke(_itemIndexReplacements);
        }
    }
}
