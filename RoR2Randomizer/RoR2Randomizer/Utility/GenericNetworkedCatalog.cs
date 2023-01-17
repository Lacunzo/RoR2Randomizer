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
        protected GenericNetworkedCatalog() : base()
        {
            NetworkingManager.RegisterMessageProvider(this, MessageProviderFlags.Persistent);
        }

        protected abstract TIdentifier createIdentifierForObject(TObject obj);

        public override TIdentifier GetIdentifier(TObject obj)
        {
            TIdentifier identifier = base.GetIdentifier(obj);
            if (!identifier.IsValid)
            {
                identifier = createIdentifierForObject(obj);
                if (NetworkServer.active)
                {
                    appendIdentifier(ref identifier, false);

#if DEBUG
                    Log.Debug(LOG_PREFIX_TYPE + $"Created {typeof(TIdentifier).Name} '{identifier}'");
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
                            Log.Warning(LOG_PREFIX_TYPE + $"catalog is networked, but {nameof(syncIdentifierNeededMessage)} is null");
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
            if (!NetworkServer.active && NetworkClient.active)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX_TYPE + $"from server");
#endif

                _identifiersCount = identifiersCount;

                ArrayUtils.EnsureCapacity(ref _identifiers, identifiersCount);
                Array.Copy(identifiers, _identifiers, identifiersCount);
            }
        }

        protected void SyncIndexNeeded_OnReceive(TIdentifier required)
        {
            if (NetworkServer.active)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX_TYPE + $"from client with identifier: {required}");
#endif

                appendIdentifier(ref required, true);
                this.TrySendAll(NetworkDestination.Clients);
            }
        }
    }
}
