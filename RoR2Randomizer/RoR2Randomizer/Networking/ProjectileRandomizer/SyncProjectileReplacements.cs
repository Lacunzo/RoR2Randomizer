using RoR2;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer
{
    public sealed class SyncProjectileReplacements : NetworkMessageBase
    {
        public delegate void OnReceiveDelegate(ReplacementDictionary<ProjectileTypeIdentifier> projectileReplacements, bool isAppendedReplacements);
        public static event OnReceiveDelegate OnReceive;

        static uint _currentAppendedServerVersion = 0;
        static uint? _latestAppendedServerVersionReceived = null;

        bool _isAppendedReplacements;
        uint _appendedServerVersion;
        ReplacementDictionary<ProjectileTypeIdentifier> _projectileReplacements;

        [SystemInitializer]
        static void Init()
        {
            Run.onRunDestroyGlobal += static _ =>
            {
                _currentAppendedServerVersion = 0;
                _latestAppendedServerVersionReceived = null;
            };
        }

        public SyncProjectileReplacements()
        {
        }

        public SyncProjectileReplacements(ReplacementDictionary<ProjectileTypeIdentifier> projectileReplacements, bool isAppendedReplacements)
        {
            if (isAppendedReplacements)
            {
                _appendedServerVersion = _currentAppendedServerVersion++;
            }

            _isAppendedReplacements = isAppendedReplacements;
            _projectileReplacements = projectileReplacements;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(_isAppendedReplacements);
            if (_isAppendedReplacements)
            {
                writer.WritePackedUInt32(_appendedServerVersion);
            }

            writer.WritePackedUInt32((uint)_projectileReplacements.Count);
            foreach (KeyValuePair<ProjectileTypeIdentifier, ProjectileTypeIdentifier> pair in _projectileReplacements)
            {
                pair.Key.Serialize(writer);
                pair.Value.Serialize(writer);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            _isAppendedReplacements = reader.ReadBoolean();
            if (_isAppendedReplacements)
            {
                _appendedServerVersion = reader.ReadPackedUInt32();
            }

            uint length = reader.ReadPackedUInt32();

            Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier> dict = new Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier>();

            for (uint i = 0; i < length; i++)
            {
                dict.Add(new ProjectileTypeIdentifier(reader), new ProjectileTypeIdentifier(reader));
            }

            _projectileReplacements = new ReplacementDictionary<ProjectileTypeIdentifier>(dict);
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

            if (_isAppendedReplacements)
            {
                if (_latestAppendedServerVersionReceived.HasValue && _appendedServerVersion <= _latestAppendedServerVersionReceived.Value)
                {
#if DEBUG
                    Log.Debug($"Discarding {nameof(SyncProjectileReplacements)} due to newer version already received {nameof(_appendedServerVersion)}={_appendedServerVersion}, {nameof(_latestAppendedServerVersionReceived)}={_latestAppendedServerVersionReceived}");
#endif
                    return;
                }

                _latestAppendedServerVersionReceived = _appendedServerVersion;
            }

            OnReceive?.Invoke(_projectileReplacements, _isAppendedReplacements);
        }
    }
}
