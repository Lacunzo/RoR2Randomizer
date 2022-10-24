using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    [RequireComponent(typeof(ProjectileController))]
    public sealed class ProjectileParentChainTracker : MonoBehaviour
    {
        ProjectileController _projectileController;

        int[] _parentIndicesChain = Array.Empty<int>();

        ProjectileParentChainTracker _parent;
        public ProjectileParentChainTracker Parent
        {
            get => _parent;
            set
            {
                _parent = value;

                int parentChainLength = value._parentIndicesChain.Length;

                if (_parentIndicesChain.Length != parentChainLength + 1)
                    _parentIndicesChain = new int[parentChainLength + 1];

                if (parentChainLength > 0)
                    Array.Copy(value._parentIndicesChain, _parentIndicesChain, parentChainLength);
                
                _parentIndicesChain[parentChainLength] = value._projectileController.catalogIndex;
            }
        }

        void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();
        }

        public void TrySetParent(GameObject parentObj)
        {
            ProjectileParentChainTracker newParent = parentObj.GetComponent<ProjectileParentChainTracker>();
            if (Parent && !newParent)
                return;
            
            Parent = newParent;
        }

        public bool IsChildOf(int catalogIndex)
        {
            return (_projectileController && _projectileController.catalogIndex == catalogIndex) || Array.IndexOf(_parentIndicesChain, catalogIndex) != -1;
        }
    }
}
