using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.RandomizerController.Projectile;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncProjectileReplacements : INetMessage
    {
        ReplacementDictionary<int> _projectileReplacements;

        public SyncProjectileReplacements()
        {
        }

        public SyncProjectileReplacements(ReplacementDictionary<int> projectileReplacements)
        {
            _projectileReplacements = projectileReplacements;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_projectileReplacements.Count);
            foreach (KeyValuePair<int, int> pair in _projectileReplacements)
            {
                writer.WritePackedIndex32(pair.Key);
                writer.WritePackedIndex32(pair.Value);
            }
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            Dictionary<int, int> projectileReplacements = new Dictionary<int, int>();

            uint count = reader.ReadPackedUInt32();
            for (int i = 0; i < count; i++)
            {
                projectileReplacements.Add(reader.ReadPackedIndex32(), reader.ReadPackedIndex32());
            }

            _projectileReplacements = new ReplacementDictionary<int>(projectileReplacements);
        }

        void INetMessage.OnReceived()
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

            ProjectileRandomizerController.OnProjectileReplacementsReceivedFromServer(_projectileReplacements);
        }
    }
}
