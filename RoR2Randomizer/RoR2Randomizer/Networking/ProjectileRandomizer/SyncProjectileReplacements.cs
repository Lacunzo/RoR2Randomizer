using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncProjectileReplacements : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(IndexReplacementsCollection projectileReplacements);

        public static event OnReceiveDelegate OnReceive;

        IndexReplacementsCollection _projectileReplacements;

        public SyncProjectileReplacements()
        {
        }

        public SyncProjectileReplacements(IndexReplacementsCollection projectileReplacements)
        {
            _projectileReplacements = projectileReplacements;
        }

        public override void Serialize(NetworkWriter writer)
        {
            _projectileReplacements.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _projectileReplacements = IndexReplacementsCollection.Deserialize(reader);
        }

        public override void OnReceived()
        {
            if (NetworkServer.active)
            {
#if DEBUG
                Log.Debug($"Received {nameof(SyncProjectileReplacements)} as server, skipping");
#endif

                return;
            }

#if DEBUG
            Log.Debug($"Received {nameof(SyncProjectileReplacements)} as client, applying replacements");
#endif

            OnReceive?.Invoke(_projectileReplacements);
        }
    }
}
