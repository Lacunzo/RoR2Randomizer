using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncBulletAttackCatalog : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(BulletAttackIdentifier[] identifiers, int identifiersCount);
        public static event OnReceiveDelegate OnReceive;

        BulletAttackIdentifier[] _identifiers;
        int _identifiersCount;

        public SyncBulletAttackCatalog()
        {
        }

        public SyncBulletAttackCatalog(BulletAttackIdentifier[] identifiers, int identifiersCount)
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

            _identifiers = new BulletAttackIdentifier[_identifiersCount = length];
            for (int i = 0; i < length; i++)
            {
                _identifiers[i] = new BulletAttackIdentifier(reader);
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
