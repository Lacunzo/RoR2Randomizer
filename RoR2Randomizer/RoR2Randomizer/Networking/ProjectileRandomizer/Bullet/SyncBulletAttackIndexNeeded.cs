using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer.Bullet
{
    public sealed class SyncBulletAttackIndexNeeded : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(BulletAttackIdentifier required);
        public static event OnReceiveDelegate OnReceive;

        BulletAttackIdentifier _required;

        public SyncBulletAttackIndexNeeded()
        {
        }

        public SyncBulletAttackIndexNeeded(in BulletAttackIdentifier required)
        {
            _required = required;
        }

        public override void Serialize(NetworkWriter writer)
        {
            _required.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _required = new BulletAttackIdentifier(reader);
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
