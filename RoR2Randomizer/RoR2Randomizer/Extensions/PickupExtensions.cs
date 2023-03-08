using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Extensions
{
    public static class PickupExtensions
    {
        static bool tryGrantTo(this PickupDef pickupDef, Inventory inventory, int count, out bool notify)
        {
            if (pickupDef == null || !inventory || count <= 0)
            {
                notify = false;
                return false;
            }

            if (pickupDef.itemIndex != ItemIndex.None)
            {
                if (inventory)
                {
                    inventory.GiveItem(pickupDef.itemIndex, count);
                    notify = true;
                }
                else
                {
                    notify = false;
                }
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                if (inventory)
                {
                    int slotCount = inventory.GetEquipmentSlotCount();

                    void giveEquipments(bool requireEmpty)
                    {
                        if (count <= 0)
                            return;

                        for (uint i = 0; i < slotCount; i++)
                        {
                            EquipmentState equipmentState = inventory.GetEquipment(i);
                            if (equipmentState.equipmentDef && (requireEmpty ? equipmentState.equipmentDef.equipmentIndex == EquipmentIndex.None
                                                                             : equipmentState.equipmentDef.equipmentIndex != pickupDef.equipmentIndex))
                            {
                                count--;
                                inventory.SetEquipmentIndexForSlot(pickupDef.equipmentIndex, i);

                                if (count <= 0)
                                    break;
                            }
                        }
                    }

                    giveEquipments(true);

                    if (count > 0)
                    {
                        giveEquipments(false);
                    }

                    notify = true;
                }
                else
                {
                    notify = false;
                }
            }
            else
            {
                Log.Warning($"pickup {pickupDef} not implemented");
                notify = false;
                return false;
            }

            return true;
        }

        public static bool TryGrantTo(this PickupDef pickupDef, Inventory inventory, int count = 1)
        {
            return pickupDef.tryGrantTo(inventory, count, out _);
        }

        public static bool TryGrantTo(this PickupDef pickupDef, CharacterMaster master, int count = 1)
        {
            if (master && pickupDef.tryGrantTo(master.inventory, count, out bool notify))
            {
                if (notify && master.playerCharacterMasterController)
                {
                    GenericPickupController.SendPickupMessage(master, pickupDef.pickupIndex);
                }

                return true;
            }

            return false;
        }

        public static int GetPickupCount(this CharacterMaster master, PickupDef pickupDef)
        {
            if (pickupDef == null || !master)
                return 0;

            Inventory inventory = master.inventory;
            if (pickupDef.itemIndex != ItemIndex.None)
            {
                if (inventory)
                {
                    return inventory.GetItemCount(pickupDef.itemIndex);
                }
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                if (inventory)
                {
                    int count = 0;

                    int equipmentSlotCount = inventory.GetEquipmentSlotCount();
                    for (uint i = 0; i < equipmentSlotCount; i++)
                    {
                        EquipmentDef equipmentDef = inventory.GetEquipment(i).equipmentDef;
                        if (equipmentDef && equipmentDef.equipmentIndex == pickupDef.equipmentIndex)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }

            return 0;
        }

        public static void TryDeductFrom(this PickupDef pickupDef, CharacterMaster master, int count = 1)
        {
            if (pickupDef == null || !master || count <= 0)
                return;

            Inventory inventory = master.inventory;
            if (pickupDef.itemIndex != ItemIndex.None)
            {
                if (inventory)
                {
                    count = Mathf.Min(count, inventory.GetItemCount(pickupDef.itemIndex));
                    if (count > 0)
                    {
                        inventory.RemoveItem(pickupDef.itemIndex, count);
                    }
                }
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                if (inventory)
                {
                    int slotCount = inventory.GetEquipmentSlotCount();
                    for (uint i = 0; i < slotCount; i++)
                    {
                        EquipmentState equipmentState = inventory.GetEquipment(i);
                        if (equipmentState.equipmentDef && equipmentState.equipmentDef.equipmentIndex == pickupDef.equipmentIndex)
                        {
                            count--;
                            inventory.SetEquipmentIndexForSlot(EquipmentIndex.None, i);

                            if (count <= 0)
                                break;
                        }
                    }
                }
            }
            else if (pickupDef.miscPickupIndex != MiscPickupIndex.None)
            {
                IReadOnlyList<MiscPickupDef> miscPickupDefs = MiscPickupCatalog.miscPickupDefs;
                if (miscPickupDefs != null)
                {
                    int miscPickupIndex = (int)pickupDef.miscPickupIndex;
                    if (miscPickupIndex >= 0 && miscPickupIndex < miscPickupDefs.Count)
                    {
                        MiscPickupDef miscPickupDef = miscPickupDefs[miscPickupIndex];
                        uint coinValue = miscPickupDef.coinValue;
                        if (miscPickupDef is LunarCoinDef lunarCoinDef)
                        {
                            NetworkUser networkUser = Util.LookUpBodyNetworkUser(master.GetBody());
                            if (networkUser)
                            {
                                networkUser.DeductLunarCoins(coinValue);
                            }
                        }
                        else if (miscPickupDef is VoidCoinDef voidCoinDef)
                        {
                            master.voidCoins -= coinValue;
                        }
                        else
                        {
                            Log.Warning($"{nameof(MiscPickupDef)} {miscPickupDef} is not implemented");
                        }
                    }
                }
            }
            else
            {
                Log.Warning($"pickup {pickupDef} not implemented");
            }
        }
    }
}
