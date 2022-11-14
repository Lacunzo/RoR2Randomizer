using HG;
using R2API.Networking;
using RoR2;
using RoR2.Orbs;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.Lightning;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.Orbs.LightningOrbHandling
{
    public class LightningOrbCatalog : INetMessageProvider
    {
        public delegate void LightningOrbIdentifierAppendedDelegate(LightningOrbIdentifier identifier);
        public static event LightningOrbIdentifierAppendedDelegate LightningOrbIdentifierAppendedServer;

        static LightningOrbCatalog _instance;

        static int _identifiersCount = 0;
        static LightningOrbIdentifier[] _identifiers = new LightningOrbIdentifier[(int)LightningOrb.LightningType.Count];

        [SystemInitializer(typeof(EntityStateCatalog))]
        static void Init()
        {
            _instance = new LightningOrbCatalog();
            NetworkingManager.RegisterMessageProvider(_instance, MessageProviderFlags.Persistent);

            static void initIdentifier(LightningOrb.LightningType lightningType, uint defaultNumBounces, uint targetsToFindPerBounce)
            {
                LightningOrbIdentifier identifier = new LightningOrbIdentifier(lightningType, defaultNumBounces, targetsToFindPerBounce);

#if DEBUG
                const bool IS_DEBUG = true;
#else
                const bool IS_DEBUG = false;
#endif
                appendIdentifier(ref identifier, IS_DEBUG);
            }

            for (LightningOrb.LightningType i = 0; i < LightningOrb.LightningType.Count; i++)
            {
                initIdentifier(i, i switch
                {
                    LightningOrb.LightningType.Ukulele => 2U,
                    LightningOrb.LightningType.Tesla => 2U,
                    LightningOrb.LightningType.HuntressGlaive => (uint)EntityStates.Croco.Disease.maxBounces,
                    LightningOrb.LightningType.Loader => 3U,
                    LightningOrb.LightningType.CrocoDisease => (uint)EntityStates.Huntress.HuntressWeapon.ThrowGlaive.maxBounceCount,
                    _ => 0U
                },
                i switch
                {
                    LightningOrb.LightningType.CrocoDisease => 2U,
                    _ => 1U
                });
            }

            SyncLightningOrbCatalog.OnReceive += SyncLightningOrbCatalog_OnReceive;
            SyncLightningOrbIndexNeeded.OnReceive += SyncLightningOrbIndexNeeded_OnReceive;
        }

        static void appendIdentifier(ref LightningOrbIdentifier identifier, bool checkExisting)
        {
            const string LOG_PREFIX = $"{nameof(LightningOrbCatalog)}.{nameof(appendIdentifier)} ";

            if (checkExisting)
            {
                for (int i = 0; i < _identifiersCount; i++)
                {
                    if (_identifiers[i].Equals(identifier, false))
                    {
#if DEBUG
                        Log.Warning(LOG_PREFIX + $"duplicate identifier {identifier}");
#endif
                        return;
                    }
                }
            }

            identifier.Index = _identifiersCount;

            ArrayUtils.ArrayAppend(ref _identifiers, ref _identifiersCount, identifier);

#if DEBUG
            Log.Debug(LOG_PREFIX + $"appended identifier {identifier}");
#endif

            if (NetworkServer.active)
            {
                LightningOrbIdentifierAppendedServer?.Invoke(identifier);
            }
        }

        public static LightningOrbIdentifier GetIdentifier(int index)
        {
            if (index < 0 || index >= _identifiersCount)
                return LightningOrbIdentifier.Invalid;

            return _identifiers[index];
        }

        static bool tryGetIdentifier(LightningOrb lightningOrb, out LightningOrbIdentifier identifier)
        {
            for (int i = 0; i < _identifiersCount; i++)
            {
                if (_identifiers[i].Matches(lightningOrb))
                {
                    identifier = _identifiers[i];
                    return true;
                }
            }

            identifier = LightningOrbIdentifier.Invalid;
            return false;
        }

        public static LightningOrbIdentifier GetIdentifier(LightningOrb lightningOrb)
        {
            if (!tryGetIdentifier(lightningOrb, out LightningOrbIdentifier identifier))
            {
                identifier = new LightningOrbIdentifier(lightningOrb);
                if (NetworkServer.active)
                {
                    appendIdentifier(ref identifier, false);

#if DEBUG
                    Log.Debug($"Created {nameof(LightningOrbIdentifier)} '{identifier}'");
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
                        new SyncLightningOrbIndexNeeded(identifier).SendTo(NetworkDestination.Server);
                    }

                    return LightningOrbIdentifier.Invalid;
                }
            }

            return identifier;
        }

        bool INetMessageProvider.SendMessages => true;

        IEnumerable<NetworkMessageBase> INetMessageProvider.GetNetMessages()
        {
            yield return new SyncLightningOrbCatalog(_identifiers, _identifiersCount);
        }

        static void SyncLightningOrbCatalog_OnReceive(LightningOrbIdentifier[] identifiers, int identifiersCount)
        {
            if (NetworkServer.active)
                return;

            if (NetworkClient.active)
            {
                _identifiersCount = identifiersCount;

                ArrayUtils.EnsureCapacity(ref _identifiers, identifiersCount);
                Array.Copy(identifiers, 0, _identifiers, 0, identifiersCount);
            }
        }

        static void SyncLightningOrbIndexNeeded_OnReceive(LightningOrbIdentifier identifier)
        {
            if (NetworkServer.active)
            {
                appendIdentifier(ref identifier, true);

                if (!NetworkServer.dontListen)
                {
                    _instance.TrySendAll(NetworkDestination.Clients);
                }
            }
        }

        public static IEnumerable<ProjectileTypeIdentifier> GetAllLightningOrbProjectileIdentifiers()
        {
            for (int i = 0; i < _identifiersCount; i++)
            {
                yield return _identifiers[i];
            }
        }
    }
}
