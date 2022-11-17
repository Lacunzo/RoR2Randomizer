using HG;
using RoR2;
using RoR2.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.Generic.Chunking;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.EffectRandomizer
{
    public sealed class SyncEffectReplacements : ChunkedNetworkMessage
    {
        public delegate void OnReceiveDelegate(IndexReplacementsCollection effectReplacements);
        public static event OnReceiveDelegate OnReceive;

        IndexReplacementsCollection _effectReplacements;

        public SyncEffectReplacements()
        {
        }

        public SyncEffectReplacements(IndexReplacementsCollection effectReplacements)
        {
            _effectReplacements = effectReplacements;
        }

        public override void Serialize(NetworkWriter writer)
        {
            _effectReplacements.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _effectReplacements = IndexReplacementsCollection.Deserialize(reader);
        }

        public override void OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncEffectReplacements)} received isServer={NetworkServer.active}, isClient={NetworkClient.active}");
#endif

            OnReceive?.Invoke(_effectReplacements);
        }
    }
}
