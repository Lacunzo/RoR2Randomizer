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

        public static readonly InitializeOnAccessDictionary<string, MasterCatalog.MasterIndex> MasterPrefabIndices = new InitializeOnAccessDictionary<string, MasterCatalog.MasterIndex>(MasterCatalog.FindMasterIndex);
    }
}
