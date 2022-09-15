using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public static class Caches
    {
        public static readonly GenericCache<CharacterMaster> MasterPrefabs = new GenericCache<CharacterMaster>(name => MasterCatalog.FindMasterPrefab(name)?.GetComponent<CharacterMaster>());

        public class GenericCache<T> : InitializeOnAccessDictionary<string, T>
        {
            public GenericCache(ValueSelectorTryGet selector) : base(selector)
            {
            }

            public GenericCache(Func<string, T> selector) : base(selector)
            {
            }

            public GenericCache(Func<T> selector) : base(selector)
            {
            }

            protected GenericCache()
            {
            }
        }
    }
}
