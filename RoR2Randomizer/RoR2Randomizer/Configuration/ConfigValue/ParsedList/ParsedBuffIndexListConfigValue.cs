using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Configuration.ConfigValue.ParsedList
{
    public class ParsedBuffIndexListConfigValue : ParsedListConfigValue<BuffIndex>
    {
        static readonly List<ParsedBuffIndexListConfigValue> _instancesWaitingForCatalogInit = new List<ParsedBuffIndexListConfigValue>();

        [SystemInitializer(typeof(BuffCatalog))]
        static void InitInstancesWaitingForCatalog()
        {
            foreach (ParsedBuffIndexListConfigValue instance in _instancesWaitingForCatalogInit)
            {
                instance.tryParseToList();
            }

            _instancesWaitingForCatalogInit.Clear();
        }

        public ParsedBuffIndexListConfigValue(ConfigEntry<string> entry) : base(entry)
        {
            if (BuffCatalog.buffDefs == null)
            {
                _instancesWaitingForCatalogInit.Add(this);
            }
        }

        protected override IEnumerable<BuffIndex> parse(string[] values)
        {
            if (BuffCatalog.buffDefs == null)
                return Enumerable.Empty<BuffIndex>();

            return values.Select(buffName =>
            {
                buffName = buffName.Trim();

                static BuffIndex findBuffIndexCaseInsensitive(string buffName)
                {
                    return BuffCatalog.buffDefs.FirstOrDefault(bd => string.Equals(bd.name, buffName, StringComparison.OrdinalIgnoreCase))?.buffIndex ?? BuffIndex.None;
                }

                BuffIndex buffIndex = findBuffIndexCaseInsensitive(buffName);
                if (buffIndex == BuffIndex.None)
                {
                    if (!buffName.StartsWith("bd"))
                    {
                        buffIndex = findBuffIndexCaseInsensitive("bd" + buffName);
                    }

                    if (buffIndex == BuffIndex.None)
                    {
                        Log.Warning($"Could not find buff with name \"{buffName}\"");
                    }
                }

                return buffIndex;
            }).Where(i => i != BuffIndex.None)
              .Distinct()
              .OrderBy(i => i);
        }
    }
}
