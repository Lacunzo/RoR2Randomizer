using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncDamageOrbIndexNeeded : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(DamageOrbIdentifier required);
        public static event OnReceiveDelegate OnReceive;

        DamageOrbIdentifier _required;

        public SyncDamageOrbIndexNeeded()
        {
        }

        public SyncDamageOrbIndexNeeded(DamageOrbIdentifier required)
        {
            _required = required;
        }

        public override void Serialize(NetworkWriter writer)
        {
            _required.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _required = new DamageOrbIdentifier(reader);
        }

        public override void OnReceived()
        {
            if (NetworkServer.active)
            {
                OnReceive?.Invoke(_required);
            }
        }
    }
}
