using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Generic
{
    public abstract class NetworkMessageBase : INetMessage
    {
        public virtual void SendTo(NetworkDestination destination)
        {
            this.Send(destination);
        }

        public virtual void SendTo(NetworkConnection connection)
        {
            this.Send(connection);
        }

        public abstract void Serialize(NetworkWriter writer);
        public abstract void Deserialize(NetworkReader reader);
        public abstract void OnReceived();
    }
}
