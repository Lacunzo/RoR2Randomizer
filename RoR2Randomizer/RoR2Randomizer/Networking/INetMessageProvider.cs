using R2API.Networking.Interfaces;
using System.Collections.Generic;

namespace RoR2Randomizer.Networking
{
    public interface INetMessageProvider
    {
        bool SendMessages { get; }

        IEnumerable<INetMessage> GetNetMessages();
    }
}
