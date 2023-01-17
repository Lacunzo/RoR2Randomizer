using RoR2;
using RoR2Randomizer.Networking.Generic.Chunking;
using RoR2Randomizer.RandomizerControllers.SurvivorPod;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.SurvivorPodRandomizer
{
    public sealed class SyncSurvivorPodReplacements : ChunkedNetworkMessage
    {
        public delegate void OnReceiveDelegate(Dictionary<BodyIndex, SpawnPodPrefabData> overrideSpawnPods);
        public static event OnReceiveDelegate OnReceive;

        Dictionary<BodyIndex, SpawnPodPrefabData> _overrideSpawnPods;

        public SyncSurvivorPodReplacements()
        {
        }

        public SyncSurvivorPodReplacements(Dictionary<BodyIndex, SpawnPodPrefabData> overrideSpawnPods)
        {
            _overrideSpawnPods = overrideSpawnPods;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_overrideSpawnPods.Count);
            foreach (KeyValuePair<BodyIndex, SpawnPodPrefabData> overridePodPair in _overrideSpawnPods)
            {
                writer.WriteBodyIndex(overridePodPair.Key);
                overridePodPair.Value.Serialize(writer);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            uint count = reader.ReadPackedUInt32();

            _overrideSpawnPods = new Dictionary<BodyIndex, SpawnPodPrefabData>();

            for (uint i = 0; i < count; i++)
            {
                _overrideSpawnPods.Add(reader.ReadBodyIndex(), new SpawnPodPrefabData(reader));
            }
        }

        public override void OnReceived()
        {
#if DEBUG
            Log.Debug($"isServer={NetworkServer.active} isClient={NetworkClient.active}");
#endif

            OnReceive?.Invoke(_overrideSpawnPods);
        }
    }
}
