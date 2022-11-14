using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.LightningOrbHandling;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.Lightning
{
    public sealed class SyncLightningOrbIndexNeeded : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(LightningOrbIdentifier identifier);
        public static event OnReceiveDelegate OnReceive;

        LightningOrbIdentifier _identifier;

        public SyncLightningOrbIndexNeeded()
        {
        }

        public SyncLightningOrbIndexNeeded(LightningOrbIdentifier identifier)
        {
            _identifier = identifier;
        }

        public override void Serialize(NetworkWriter writer)
        {
            _identifier.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _identifier = new LightningOrbIdentifier(reader);
        }

        public override void OnReceived()
        {
            OnReceive?.Invoke(_identifier);
        }
    }
}
