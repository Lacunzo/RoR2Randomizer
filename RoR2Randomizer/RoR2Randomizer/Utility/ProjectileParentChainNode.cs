using R2API.Networking.Interfaces;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public class ProjectileParentChainNode : ISerializableObject
    {
        public ProjectileTypeIdentifier NodeIdentifier { get; private set; }
        public ProjectileParentChainNode Parent;

        public ProjectileParentChainNode(ProjectileTypeIdentifier nodeIdentifier)
        {
            NodeIdentifier = nodeIdentifier;
        }

        public ProjectileParentChainNode()
        {
        }

        public IEnumerable<ProjectileTypeIdentifier> ConstructChain()
        {
            return MiscUtils.CreateReverseLinkedListFromLinks(this, static t => t.Parent).Select(static n => n.NodeIdentifier);
        }

        public bool IsChildOf(ProjectileTypeIdentifier identifier)
        {
            return identifier.IsValid && ConstructChain().Contains(identifier);
        }

        public void Deserialize(NetworkReader reader)
        {
            NodeIdentifier = new ProjectileTypeIdentifier(reader);

            if (reader.ReadBoolean())
            {
                Parent = new ProjectileParentChainNode();
                Parent.Deserialize(reader);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            NodeIdentifier.Serialize(writer);

            bool hasParent = Parent != null;
            writer.Write(hasParent);
            if (hasParent)
            {
                Parent.Serialize(writer);
            }
        }
    }
}
