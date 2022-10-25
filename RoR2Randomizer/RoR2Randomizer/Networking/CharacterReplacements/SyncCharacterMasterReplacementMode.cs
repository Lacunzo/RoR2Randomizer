using RoR2;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.CharacterReplacements
{
    public sealed class SyncCharacterMasterReplacementMode : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(CharacterReplacementMode mode);
        public static event OnReceiveDelegate OnReceive;

        CharacterReplacementMode _mode;

        public SyncCharacterMasterReplacementMode()
        {
        }

        public SyncCharacterMasterReplacementMode(CharacterReplacementMode mode)
        {
            _mode = mode;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedIndex32((int)_mode);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _mode = (CharacterReplacementMode)reader.ReadPackedIndex32();
        }

        public override void OnReceived()
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                OnReceive?.Invoke(_mode);
            }
        }
    }
}
