using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncDamageOrbCatalog : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(DamageOrbIdentifier[] identifiers, int identifiersCount);
        public static event OnReceiveDelegate OnReceive;

        DamageOrbIdentifier[] _identifiers;
        int _identifiersCount;

        public SyncDamageOrbCatalog()
        {
        }

        public SyncDamageOrbCatalog(DamageOrbIdentifier[] identifiers, int identifiersCount)
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
            int length = (int)reader.ReadPackedUInt32();

            _identifiers = new DamageOrbIdentifier[_identifiersCount = length];
            for (int i = 0; i < length; i++)
            {
                _identifiers[i] = new DamageOrbIdentifier(reader);
            }
        }

        public override void OnReceived()
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                OnReceive?.Invoke(_identifiers, _identifiersCount);
            }
        }
    }
}
