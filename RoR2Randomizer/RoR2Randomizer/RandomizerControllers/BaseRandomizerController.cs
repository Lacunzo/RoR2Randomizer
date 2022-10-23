using R2API.Networking.Interfaces;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers
{
    public abstract class BaseRandomizerController : MonoBehaviour, INetMessageProvider
    {
        public abstract bool IsRandomizerEnabled { get; }

        protected abstract bool isNetworked { get; }

        bool INetMessageProvider.SendMessages => IsRandomizerEnabled;

        protected virtual void Awake()
        {
            if (isNetworked)
            {
                NetworkingManager.RegisterMessageProvider(this, MessageProviderFlags.Persistent);
            }
        }

        IEnumerable<NetworkMessageBase> INetMessageProvider.GetNetMessages()
        {
            return getNetMessages();
        }
        protected virtual IEnumerable<NetworkMessageBase> getNetMessages()
        {
            yield break;
        }
    }
}
