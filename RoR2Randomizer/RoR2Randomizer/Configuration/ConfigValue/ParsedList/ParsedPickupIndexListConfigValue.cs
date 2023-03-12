using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.Utility.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Configuration.ConfigValue.ParsedList
{
    public class ParsedPickupIndexListConfigValue : ParsedListConfigValue<PickupIndex>
    {
        static readonly List<ParsedPickupIndexListConfigValue> _instancesWaitingForCatalogInit = new List<ParsedPickupIndexListConfigValue>();

        [SystemInitializer(typeof(PickupCatalog))]
        static void InitInstancesWaitingForCatalog()
        {
            foreach (ParsedPickupIndexListConfigValue instance in _instancesWaitingForCatalogInit)
            {
                instance.tryParseToList();
            }

            _instancesWaitingForCatalogInit.Clear();
        }

        public ParsedPickupIndexListConfigValue(ConfigEntry<string> entry) : base(entry)
        {
            if (PickupCatalog.pickupCount == 0)
            {
                _instancesWaitingForCatalogInit.Add(this);
            }
        }

        public static PickupIndex FindPickupIndexCaseInsensitive(string pickupName)
        {
            return PickupCatalog.allPickups.FirstOrDefault(pd => string.Equals(pd.internalName, pickupName, StringComparison.OrdinalIgnoreCase))?.pickupIndex ?? PickupIndex.none;
        }

        protected override IEnumerable<PickupIndex> parse(string[] values)
        {
            if (PickupCatalog.pickupCount == 0)
                return Enumerable.Empty<PickupIndex>();

            return values.Select(pickupName =>
            {
                pickupName = pickupName.Trim();

                PickupIndex pickupIndex = FindPickupIndexCaseInsensitive(pickupName);
                if (pickupIndex.isValid)
                    return pickupIndex;

                ItemIndex itemIndex = ParsedItemIndexListConfigValue.FindItemIndexCaseInsensitive(pickupName);
                if (itemIndex != ItemIndex.None)
                    return PickupCatalog.FindPickupIndex(itemIndex);

                EquipmentIndex equipmentIndex = ParsedEquipmentIndexListConfigValue.FindEquipmentIndexCaseInsensitive(pickupName);
                if (equipmentIndex != EquipmentIndex.None)
                    return PickupCatalog.FindPickupIndex(equipmentIndex);

                Log.Warning($"Could not find pickup with name \"{pickupName}\"");
                return PickupIndex.none;
            }).Where(i => i.isValid)
              .Distinct()
              .OrderBy(i => i, PickupIndexComparer.Instance);
        }
    }
}
