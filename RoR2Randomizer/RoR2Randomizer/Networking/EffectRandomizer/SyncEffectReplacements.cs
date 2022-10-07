using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.EffectRandomizer
{
    public sealed class SyncEffectReplacements : INetMessage
    {
        public delegate void OnReceiveDelegate(ReplacementDictionary<EffectIndex> effectReplacements);
        public static event OnReceiveDelegate OnReceived;

        ReplacementDictionary<EffectIndex> _effectReplacements;

        public SyncEffectReplacements()
        {
        }

        public SyncEffectReplacements(ReplacementDictionary<EffectIndex> effectReplacements)
        {
            _effectReplacements = effectReplacements;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_effectReplacements.Count);
            foreach (KeyValuePair<EffectIndex, EffectIndex> pair in _effectReplacements)
            {
                writer.WriteEffectIndex(pair.Key);
                writer.WriteEffectIndex(pair.Value);
            }
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            Dictionary<EffectIndex, EffectIndex> dict = new Dictionary<EffectIndex, EffectIndex>();

            uint count = reader.ReadPackedUInt32();
            for (uint i = 0; i < count; i++)
            {
                dict.Add(reader.ReadEffectIndex(), reader.ReadEffectIndex());
            }

            _effectReplacements = new ReplacementDictionary<EffectIndex>(dict);
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncEffectReplacements)} received");
#endif

            if (!NetworkServer.active && NetworkClient.active)
            {
#if DEBUG
                Log.Debug($"{nameof(SyncEffectReplacements)} received as client, invoking event");
#endif

                OnReceived?.Invoke(_effectReplacements);
            }
#if DEBUG
            else
            {
                Log.Debug($"{nameof(SyncEffectReplacements)} received as server, skipping");
            }
#endif
        }
    }
}
