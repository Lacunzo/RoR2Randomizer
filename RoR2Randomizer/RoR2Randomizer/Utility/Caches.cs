using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public static class Caches
    {
        public static readonly InitializeOnAccessDictionary<string, CharacterMaster> MasterPrefabs = new InitializeOnAccessDictionary<string, CharacterMaster>(name => MasterCatalog.FindMasterPrefab(name)?.GetComponent<CharacterMaster>());

        public static readonly InitializeOnAccessDictionary<GameObject, float> CharacterBodyRadius = new InitializeOnAccessDictionary<GameObject, float>((GameObject bodyPrefab, out float radius) =>
        {
            if (bodyPrefab.TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
            {
                radius = sphereCollider.radius;
                return true;
            }
            else if (bodyPrefab.TryGetComponent<CapsuleCollider>(out CapsuleCollider capsuleCollider))
            {
                radius = capsuleCollider.radius;
                return true;
            }
            else
            {
                radius = -1f;
                return false;
            }
        });
    }
}
