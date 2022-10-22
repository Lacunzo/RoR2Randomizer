using R2API.Networking.Interfaces;
using RoR2Randomizer.Networking;
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
                NetworkingManager.RegisterMessageProvider(this);
            }
        }

        IEnumerable<INetMessage> INetMessageProvider.GetNetMessages()
        {
            return getNetMessages();
        }
        protected virtual IEnumerable<INetMessage> getNetMessages()
        {
            yield break;
        }
    }
}
