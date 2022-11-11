using HG;
using R2API.Networking;
using RoR2;
using RoR2.Orbs;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.DamageOrbHandling
{
    public class DamageOrbCatalog : INetMessageProvider
    {
        static DamageOrbCatalog _instance;

        [SystemInitializer]
        static void Init()
        {
            _instance = new DamageOrbCatalog();
            NetworkingManager.RegisterMessageProvider(_instance, MessageProviderFlags.Persistent);

            SyncDamageOrbCatalog.OnReceive += SyncDamageOrbCatalog_OnReceive;
            SyncDamageOrbIndexNeeded.OnReceive += SyncDamageOrbIndexNeeded_OnReceive;
        }

        public static event Action<DamageOrbIdentifier> DamageOrbAppendedServer;

        static int _damageOrbsCount = 0;
        static DamageOrbIdentifier[] _damageOrbsByIndex = new DamageOrbIdentifier[10];

        static void appendIdentifier(ref DamageOrbIdentifier identifier, bool checkExisting)
        {
            const string LOG_PREFIX = $"{nameof(DamageOrbCatalog)}.{nameof(appendIdentifier)} ";

            if (checkExisting)
            {
                for (int i = 0; i < _damageOrbsCount; i++)
                {
                    if (_damageOrbsByIndex[i].Equals(identifier, false))
                    {
#if DEBUG
                        Log.Warning(LOG_PREFIX + $"duplicate damage orb identifier {identifier}");
#endif

                        return;
                    }
                }
            }

            identifier.Index = _damageOrbsCount;

#if DEBUG
            Log.Debug(LOG_PREFIX + $"appended {identifier}");
#endif

            ArrayUtils.ArrayAppend(ref _damageOrbsByIndex, ref _damageOrbsCount, identifier);

            if (NetworkServer.active)
            {
                DamageOrbAppendedServer?.Invoke(identifier);
            }
        }

        static bool tryGetIdentifier(GenericDamageOrb damageOrb, out DamageOrbIdentifier identifier)
        {
            for (int i = 0; i < _damageOrbsCount; i++)
            {
                if (_damageOrbsByIndex[i].Matches(damageOrb))
                {
                    identifier = _damageOrbsByIndex[i];
                    return true;
                }
            }

            identifier = default;
            return false;
        }

        public static DamageOrbIdentifier GetIdentifier(GenericDamageOrb damageOrb)
        {
            if (!tryGetIdentifier(damageOrb, out DamageOrbIdentifier identifier))
            {
                identifier = new DamageOrbIdentifier(damageOrb);
                if (NetworkServer.active)
                {
                    appendIdentifier(ref identifier, false);

#if DEBUG
                    Log.Debug($"Created {nameof(DamageOrbIdentifier)} '{identifier}'");
#endif

                    if (!NetworkServer.dontListen)
                    {
                        _instance.TrySendAll(NetworkDestination.Clients);
                    }
                }
                else
                {
                    if (NetworkClient.active)
                    {
                        new SyncDamageOrbIndexNeeded(identifier).SendTo(NetworkDestination.Server);
                    }

                    return DamageOrbIdentifier.Invalid;
                }
            }

            return identifier;
        }

        public static DamageOrbIdentifier GetIdentifier(int index)
        {
            if (index < 0 || index >= _damageOrbsCount)
                return DamageOrbIdentifier.Invalid;

            return _damageOrbsByIndex[index];
        }

        bool INetMessageProvider.SendMessages => true;

        IEnumerable<NetworkMessageBase> INetMessageProvider.GetNetMessages()
        {
            yield return new SyncDamageOrbCatalog(_damageOrbsByIndex, _damageOrbsCount);
        }

        static void SyncDamageOrbCatalog_OnReceive(DamageOrbIdentifier[] identifiers, int identifiersCount)
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                _damageOrbsCount = identifiersCount;

                ArrayUtils.EnsureCapacity(ref _damageOrbsByIndex, identifiersCount);
                Array.Copy(identifiers, _damageOrbsByIndex, identifiersCount);
            }
        }

        static void SyncDamageOrbIndexNeeded_OnReceive(DamageOrbIdentifier required)
        {
            if (NetworkServer.active)
            {
                appendIdentifier(ref required, true);
                _instance.TrySendAll(NetworkDestination.Clients);
            }
        }

        public static IEnumerable<ProjectileTypeIdentifier> GetAllDamageOrbProjectileIdentifiers()
        {
            for (int i = 0; i < _damageOrbsCount; i++)
            {
                yield return _damageOrbsByIndex[i];
            }
        }
    }
}
