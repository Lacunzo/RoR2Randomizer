using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.RandomizerControllers;
using RoR2Randomizer.Utility.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Configuration.ConfigValue.ParsedList
{
    public class ParsedMasterIndexListConfigValue : ParsedListConfigValue<MasterCatalog.MasterIndex>
    {
        static readonly List<ParsedMasterIndexListConfigValue> _instancesWaitingForCatalogInit = new List<ParsedMasterIndexListConfigValue>();

        [SystemInitializer(typeof(MasterCatalog))]
        static void InitInstancesWaitingForCatalog()
        {
            foreach (ParsedMasterIndexListConfigValue instance in _instancesWaitingForCatalogInit)
            {
                instance.tryParseToList();
            }

            _instancesWaitingForCatalogInit.Clear();
        }

        public ParsedMasterIndexListConfigValue(ConfigEntry<string> entry) : base(entry)
        {
            if (MasterCatalog.allMasters == null)
            {
                _instancesWaitingForCatalogInit.Add(this);
            }
        }

        protected override IEnumerable<MasterCatalog.MasterIndex> parse(string[] values)
        {
            // If the catalog is not initialized
            if (MasterCatalog.allMasters == null)
                return Enumerable.Empty<MasterCatalog.MasterIndex>();

            return values.Select(masterName =>
            {
                masterName = masterName.Trim();

                static MasterCatalog.MasterIndex findMasterIndexCaseInsensitive(string masterName)
                {
                    CharacterMaster masterPrefab = MasterCatalog.allMasters.FirstOrDefault(m => string.Equals(m.name, masterName, StringComparison.OrdinalIgnoreCase));

                    if (!masterPrefab || !CharacterReplacements.ShouldRandomizeMaster(masterPrefab))
                        return MasterCatalog.MasterIndex.none;

                    return masterPrefab.masterIndex;
                }

                MasterCatalog.MasterIndex masterIndex = findMasterIndexCaseInsensitive(masterName);
                if (!masterIndex.isValid)
                {
                    if (!masterName.EndsWith("Master"))
                    {
                        masterIndex = findMasterIndexCaseInsensitive(masterName + "Master");
                    }

                    if (!masterIndex.isValid)
                    {
                        if (!masterName.EndsWith("MonsterMaster"))
                        {
                            masterIndex = findMasterIndexCaseInsensitive(masterName + "MonsterMaster");
                        }
                    }

                    if (!masterIndex.isValid)
                    {
                        Log.Warning($"Could not find master index with name \"{masterName}\"");
                    }
                }

                return masterIndex;
            }).Where(i => i.isValid)
              .Distinct()
              .OrderBy(i => i, MasterIndexComparer.Instance);
        }
    }
}
