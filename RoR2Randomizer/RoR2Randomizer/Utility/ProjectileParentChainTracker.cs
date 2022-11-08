using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    [RequireComponent(typeof(ProjectileController))]
    public sealed class ProjectileParentChainTracker : MonoBehaviour
    {
        public ProjectileParentChainNode ChainNode { get; private set; }

        void Awake()
        {
            ProjectileController projectileController = GetComponent<ProjectileController>();
            ChainNode = new ProjectileParentChainNode(new ProjectileTypeIdentifier(ProjectileType.OrdinaryProjectile, projectileController.catalogIndex));
        }

        public void TrySetParent(GameObject parentObj)
        {
            if (parentObj.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker chainTracker))
            {
                SetParent(chainTracker.ChainNode);
            }
        }

        public void SetParent(ProjectileParentChainNode node)
        {
            ChainNode.Parent = node;
        }

        public bool IsChildOf(ProjectileTypeIdentifier identifier)
        {
            return ChainNode != null && ChainNode.IsChildOf(identifier);
        }
    }
}
