#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.HoldoutZoneRandomizer
{
    public sealed class SyncHoldoutZoneReplacements : INetMessage
    {
        public delegate void OnReceivedDelegate(ReplacementDictionary<HoldoutZoneType> zoneReplacements);
        public static event OnReceivedDelegate OnReceive;

        ReplacementDictionary<HoldoutZoneType> _zoneReplacements;

        public SyncHoldoutZoneReplacements()
        {
        }

        public SyncHoldoutZoneReplacements(ReplacementDictionary<HoldoutZoneType> zoneReplacements)
        {
            _zoneReplacements = zoneReplacements;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_zoneReplacements.Count);

            foreach (KeyValuePair<HoldoutZoneType, HoldoutZoneType> pair in _zoneReplacements)
            {
                writer.WritePackedIndex32((int)pair.Key);
                writer.WritePackedIndex32((int)pair.Value);
            }
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            Dictionary<HoldoutZoneType, HoldoutZoneType> dict = new Dictionary<HoldoutZoneType, HoldoutZoneType>();

            uint count = reader.ReadPackedUInt32();
            for (uint i = 0; i < count; i++)
            {
                dict.Add((HoldoutZoneType)reader.ReadPackedIndex32(), (HoldoutZoneType)reader.ReadPackedIndex32());
            }

            _zoneReplacements = new ReplacementDictionary<HoldoutZoneType>(dict);
        }

        void INetMessage.OnReceived()
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                OnReceive?.Invoke(_zoneReplacements);
            }
        }
    }
}
#endif