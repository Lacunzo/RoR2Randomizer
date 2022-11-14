using RoR2Randomizer.Networking.Generic;
using System.Collections.Generic;

namespace RoR2Randomizer.Networking
{
    public interface INetMessageProvider
    {
        bool SendMessages { get; }

        IEnumerable<NetworkMessageBase> GetNetMessages();
    }
}
