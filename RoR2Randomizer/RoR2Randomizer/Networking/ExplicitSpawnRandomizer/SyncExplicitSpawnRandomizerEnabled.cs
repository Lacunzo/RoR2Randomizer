using RoR2Randomizer.Networking.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ExplicitSpawnRandomizer
{
    public sealed class SyncExplicitSpawnRandomizerEnabled : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(bool isEnabled);
        public static event OnReceiveDelegate OnReceive;

        bool _isEnabled;

        public SyncExplicitSpawnRandomizerEnabled()
        {
        }

        public SyncExplicitSpawnRandomizerEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(_isEnabled);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _isEnabled = reader.ReadBoolean();
        }

        public override void OnReceived()
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                OnReceive?.Invoke(_isEnabled);
            }
        }
    }
}