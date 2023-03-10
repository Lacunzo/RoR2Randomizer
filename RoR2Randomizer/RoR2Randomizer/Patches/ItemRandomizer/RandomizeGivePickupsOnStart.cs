using HG;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerControllers.Item;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class RandomizeGivePickupsOnStart
    {
        static void Apply()
        {
            On.RoR2.GivePickupsOnStart.Start += GivePickupsOnStart_Start;
        }

        static void Cleanup()
        {
            On.RoR2.GivePickupsOnStart.Start -= GivePickupsOnStart_Start;
        }

        static void GivePickupsOnStart_Start(On.RoR2.GivePickupsOnStart.orig_Start orig, GivePickupsOnStart self)
        {
            if (ItemRandomizerController.IsEnabled)
            {
                if (!self.equipmentDef && !string.IsNullOrEmpty(self.equipmentString))
                {
                    EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(self.equipmentString);
                    if (equipmentIndex != EquipmentIndex.None)
                    {
                        self.equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                    }
                    else
                    {
                        Log.Warning($"Unable to find equipment index with name \"{self.equipmentString}\"");
                    }
                }

                if (self.itemInfos.Length > 0)
                {
                    List<GivePickupsOnStart.ItemDefInfo> convertedItemDefInfos = new List<GivePickupsOnStart.ItemDefInfo>(self.itemInfos.Length);

                    foreach (GivePickupsOnStart.ItemInfo itemInfo in self.itemInfos)
                    {
                        ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemInfo.itemString);
                        if (itemIndex == ItemIndex.None)
                        {
                            Log.Warning($"Unable to find item index with name \"{itemInfo.itemString}\"");
                            continue;
                        }

                        convertedItemDefInfos.Add(new GivePickupsOnStart.ItemDefInfo
                        {
                            itemDef = ItemCatalog.GetItemDef(itemIndex),
                            count = itemInfo.count,
                            dontExceedCount = false
                        });
                    }

                    MiscUtils.AddItems(ref self.itemDefInfos, convertedItemDefInfos);
                    self.itemInfos = Array.Empty<GivePickupsOnStart.ItemInfo>();
                }

                int equipmentCount = self.equipmentDef ? 1 : 0;

                PickupIndex[] randomizedPickups = new PickupIndex[equipmentCount + self.itemDefInfos.Length];

                if (self.equipmentDef)
                {
                    randomizedPickups[0] = ItemRandomizerController.GetReplacementPickupIndex(PickupCatalog.FindPickupIndex(self.equipmentDef.equipmentIndex));
                }

                for (int i = 0; i < self.itemDefInfos.Length; i++)
                {
                    randomizedPickups[equipmentCount + i] = ItemRandomizerController.GetReplacementPickupIndex(PickupCatalog.FindPickupIndex(self.itemDefInfos[i].itemDef.itemIndex));
                }

                self.equipmentDef = null;

                List<GivePickupsOnStart.ItemDefInfo> randomizedItems = new List<GivePickupsOnStart.ItemDefInfo>(randomizedPickups.Length);

                for (int i = 0; i < randomizedPickups.Length; i++)
                {
                    if (!randomizedPickups[i].isValid)
                        continue;

                    PickupDef pickupDef = randomizedPickups[i].pickupDef;
                    if (pickupDef.IsItem())
                    {
                        GivePickupsOnStart.ItemDefInfo itemDefInfo;
                        if (ArrayUtils.IsInBounds(self.itemDefInfos, i - equipmentCount))
                        {
                            itemDefInfo = self.itemDefInfos[i - equipmentCount];
                        }
                        else
                        {
                            itemDefInfo = new GivePickupsOnStart.ItemDefInfo
                            {
                                count = 1,
                                dontExceedCount = false
                            };
                        }

                        itemDefInfo.itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);

                        randomizedItems.Add(itemDefInfo);
                    }
                    else if (pickupDef.IsEquipment())
                    {
                        self.equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                    }
                    else
                    {
                        Log.Warning($"Unimplemented pickup {pickupDef.pickupIndex}");
                    }
                }

                self.itemDefInfos = randomizedItems.ToArray();

#if DEBUG
                Log.Debug($"Randomized {nameof(GivePickupsOnStart)}: {self.name}");
#endif
            }

            orig(self);
        }
    }
}
