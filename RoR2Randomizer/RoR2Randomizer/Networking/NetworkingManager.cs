using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking
{
    public static class NetworkingManager
    {
        static readonly List<INetMessageProvider> _messageProviders = new List<INetMessageProvider>();

        static ulong? _runCallbacksHandle;

        public static void RegisterMessageProvider(INetMessageProvider provider)
        {
            _messageProviders.Add(provider);
        }

        [SystemInitializer]
        static void Init()
        {
            _runCallbacksHandle = RunSpecificCallbacksManager.AddEntry(runStart, null, -1);

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
            if (NetworkServer.active && Run.instance)
            {
                foreach (INetMessage message in collectAllMessages())
                {
                    message.Send(networkUser.connectionToClient);
                }
            }
        }

        static IEnumerable<INetMessage> collectAllMessages()
        {
            foreach (INetMessageProvider messageProvider in _messageProviders)
            {
                if (messageProvider.SendMessages)
                {
                    foreach (INetMessage message in messageProvider.GetNetMessages())
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

            foreach (INetMessage message in collectAllMessages())
            {
                message.Send(NetworkDestination.Clients);
            }
        }
    }
}
