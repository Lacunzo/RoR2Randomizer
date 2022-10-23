using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking
{
    public static class NetworkingManager
    {
        readonly struct MessageProviderInfo
        {
            public readonly INetMessageProvider Provider;
            public readonly MessageProviderFlags Flags;

            public MessageProviderInfo(INetMessageProvider provider, MessageProviderFlags flags)
            {
                Provider = provider;
                Flags = flags;
            }

            public override bool Equals(object obj)
            {
                return obj is MessageProviderInfo info &&
                       EqualityComparer<INetMessageProvider>.Default.Equals(Provider, info.Provider);
            }

            public override int GetHashCode()
            {
                return Provider.GetHashCode();
            }

            public static readonly IEqualityComparer<MessageProviderInfo> EqualityComparer = new InstanceEqualityComparer();

            class InstanceEqualityComparer : IEqualityComparer<MessageProviderInfo>
            {
                public bool Equals(MessageProviderInfo x, MessageProviderInfo y)
                {
                    return x.Equals(y);
                }

                public int GetHashCode(MessageProviderInfo obj)
                {
                    return obj.GetHashCode();
                }
            }
        }

        static readonly HashSet<MessageProviderInfo> _messageProviders = new HashSet<MessageProviderInfo>(MessageProviderInfo.EqualityComparer);

        static ulong? _runCallbacksHandle;

        public static void RegisterMessageProvider(INetMessageProvider provider, MessageProviderFlags flags = MessageProviderFlags.None)
        {
            if (!_messageProviders.Add(new MessageProviderInfo(provider, flags)))
            {
                Log.Warning($"Message provider {provider} already registered");
            }
        }

        public static void UnregisterMessageProvider(INetMessageProvider provider)
        {
            _messageProviders.RemoveWhere(i => i.Provider == provider);
        }

        [SystemInitializer]
        static void Init()
        {
            _runCallbacksHandle = RunSpecificCallbacksManager.AddEntry(runStart, static _ => _messageProviders.RemoveWhere(static i => (i.Flags & MessageProviderFlags.Persistent) == 0), -1);

            NetworkUser.onPostNetworkUserStart += NetworkUser_onPostNetworkUserStart;
        }

        internal static void Uninitialize()
        {
            if (_runCallbacksHandle.HasValue)
            {
                RunSpecificCallbacksManager.RemoveEntry(_runCallbacksHandle.Value);
            }

            NetworkUser.onPostNetworkUserStart -= NetworkUser_onPostNetworkUserStart;
        }

        static void NetworkUser_onPostNetworkUserStart(NetworkUser networkUser)
        {
            // Run instance exists, this client joined mid-run
            if (NetworkServer.active && Run.instance)
            {
                foreach (NetworkMessageBase message in collectAllMessages(false))
                {
                    message.SendTo(networkUser.connectionToClient);
                }
            }
        }

        static IEnumerable<NetworkMessageBase> collectAllMessages(bool requirePersistent)
        {
            foreach (MessageProviderInfo providerInfo in _messageProviders)
            {
                if (providerInfo.Provider.SendMessages && (!requirePersistent || (providerInfo.Flags & MessageProviderFlags.Persistent) != 0))
                {
                    foreach (NetworkMessageBase message in providerInfo.Provider.GetNetMessages())
                    {
                        yield return message;
                    }
                }
            }
        }

        static void runStart(Run _)
        {
            if (!NetworkServer.active)
                return;

            foreach (NetworkMessageBase message in collectAllMessages(true))
            {
                message.SendTo(NetworkDestination.Clients);
            }
        }
    }
}
