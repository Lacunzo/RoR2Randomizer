using HG;
using R2API.Networking;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public abstract class GenericNetworkedCatalog<TObject, TIdentifier> : GenericCatalog<TObject, TIdentifier>, INetMessageProvider where TIdentifier : ICatalogIdentifier<TObject, TIdentifier>
    {
        static readonly string LOG_PREFIX_TYPE = $"{nameof(GenericNetworkedCatalog<TObject, TIdentifier>)}<{typeof(TObject).Name}, {typeof(TIdentifier).Name}>";

        protected GenericNetworkedCatalog() : base()
        {
            NetworkingManager.RegisterMessageProvider(this, MessageProviderFlags.Persistent);
        }

        protected abstract TIdentifier createIdentifierForObject(TObject obj);

        public override TIdentifier GetIdentifier(TObject obj)
        {
            string LOG_PREFIX = $"{LOG_PREFIX_TYPE}.{nameof(GetIdentifier)} ";

            TIdentifier identifier = base.GetIdentifier(obj);
            if (!identifier.IsValid)
            {
                identifier = createIdentifierForObject(obj);
                if (NetworkServer.active)
                {
                    appendIdentifier(ref identifier, false);

#if DEBUG
                    Log.Debug(LOG_PREFIX + $"Created {typeof(TIdentifier).Name} '{identifier}'");
#endif

                    if (!NetworkServer.dontListen)
                    {
                        this.TrySendAll(NetworkDestination.Clients);
                    }
                }
                else
                {
                    if (NetworkClient.active)
                    {
                        NetworkMessageBase syncIdentifierNeededMessage = getSyncIdentifierNeededMessage(identifier);
                        if (syncIdentifierNeededMessage != null)
                        {
                            syncIdentifierNeededMessage.SendTo(NetworkDestination.Server);
                        }
                        else
                        {
                            Log.Warning(LOG_PREFIX + $"catalog is networked, but {nameof(syncIdentifierNeededMessage)} is null");
                        }
                    }

                    return InvalidIdentifier;
                }
            }

            return identifier;
        }

        protected abstract NetworkMessageBase getSyncIdentifierNeededMessage(in TIdentifier identifier);

        public virtual bool SendMessages => true;

        public abstract IEnumerable<NetworkMessageBase> GetNetMessages();

        protected void SyncCatalog_OnReceive(TIdentifier[] identifiers, int identifiersCount)
        {
#if DEBUG
            string LOG_PREFIX = $"{LOG_PREFIX_TYPE}.{nameof(SyncCatalog_OnReceive)} ";
#endif

            if (!NetworkServer.active && NetworkClient.active)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"from server");
#endif

                _identifiersCount = identifiersCount;

                ArrayUtils.EnsureCapacity(ref _identifiers, identifiersCount);
                Array.Copy(identifiers, _identifiers, identifiersCount);
            }
        }

        protected void SyncIndexNeeded_OnReceive(TIdentifier required)
        {
#if DEBUG
            string LOG_PREFIX = $"{LOG_PREFIX_TYPE}.{nameof(SyncIndexNeeded_OnReceive)} ";
#endif

            if (NetworkServer.active)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"from client with identifier: {required}");
#endif

                appendIdentifier(ref required, true);
                this.TrySendAll(NetworkDestination.Clients);
            }
        }
    }
}
