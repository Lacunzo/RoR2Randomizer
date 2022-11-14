using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.LightningOrbHandling;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.Lightning
{
    public sealed class SyncLightningOrbCatalog : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(LightningOrbIdentifier[] identifiers, int identifiersCount);
        public static event OnReceiveDelegate OnReceive;

        int _identifiersCount;
        LightningOrbIdentifier[] _identifiers;

        public SyncLightningOrbCatalog()
        {
        }

        public SyncLightningOrbCatalog(LightningOrbIdentifier[] identifiers, int identifiersCount)
        {
            _identifiers = identifiers;
            _identifiersCount = identifiersCount;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_identifiersCount);
            for (int i = 0; i < _identifiersCount; i++)
            {
                _identifiers[i].Serialize(writer);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            _identifiersCount = (int)reader.ReadPackedUInt32();

            _identifiers = new LightningOrbIdentifier[_identifiersCount];
            for (uint i = 0; i < _identifiersCount; i++)
            {
                _identifiers[i] = new LightningOrbIdentifier(reader);
            }
        }

        public override void OnReceived()
        {
            OnReceive?.Invoke(_identifiers, _identifiersCount);
        }
    }
}
