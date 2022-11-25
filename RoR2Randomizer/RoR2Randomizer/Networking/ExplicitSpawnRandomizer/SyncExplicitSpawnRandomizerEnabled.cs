using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Utility;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ExplicitSpawnRandomizer
{
    public sealed class SyncExplicitSpawnRandomizerEnabled : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(bool isEnabled, bool randomizeHeretic);
        public static event OnReceiveDelegate OnReceive;

        bool _isEnabled;
        bool _randomizeHeretic;

        public SyncExplicitSpawnRandomizerEnabled()
        {
        }

        public SyncExplicitSpawnRandomizerEnabled(bool isEnabled, bool randomizeHeretic)
        {
            _isEnabled = isEnabled;
            _randomizeHeretic = randomizeHeretic;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WriteBits(_isEnabled, _randomizeHeretic);
        }

        public override void Deserialize(NetworkReader reader)
        {
            reader.ReadBits(out _isEnabled, out _randomizeHeretic);
        }

        public override void OnReceived()
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                OnReceive?.Invoke(_isEnabled, _randomizeHeretic);
            }
        }
    }
}