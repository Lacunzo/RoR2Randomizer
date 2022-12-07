using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Extensions
{
    public static class PickupExtensions
    {
        public static void TryGrantTo(this PickupDef pickupDef, CharacterMaster master, int count = 1)
        {
            const string LOG_PREFIX = $"{nameof(PickupExtensions)}.{nameof(TryGrantTo)} ";

            if (pickupDef == null || !master || count <= 0)
                return;

            Inventory inventory = master.inventory;

            bool notify;
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
                            if (equipmentState.equipmentDef && requireEmpty ? equipmentState.equipmentDef.equipmentIndex == EquipmentIndex.None
                                                                            : equipmentState.equipmentDef.equipmentIndex != pickupDef.equipmentIndex)
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
            else if (pickupDef.miscPickupIndex != MiscPickupIndex.None)
            {
                notify = false;

                IReadOnlyList<MiscPickupDef> miscPickupDefs = MiscPickupCatalog.miscPickupDefs;
                if (miscPickupDefs != null)
                {
                    int miscPickupIndex = (int)pickupDef.miscPickupIndex;
                    if (miscPickupIndex >= 0 && miscPickupIndex < miscPickupDefs.Count)
                    {
                        PickupDef.GrantContext context = new PickupDef.GrantContext { body = master.GetBody() };
                        miscPickupDefs[miscPickupIndex].GrantPickup(ref context);

                        notify = context.shouldNotify;
                    }
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"pickup {pickupDef} not implemented");
                return;
            }

            if (notify)
            {
                GenericPickupController.SendPickupMessage(master, pickupDef.pickupIndex);
            }
        }

        public static int GetPickupCount(this CharacterMaster master, PickupDef pickupDef)
        {
            const string LOG_PREFIX = $"{nameof(PickupExtensions)}.{nameof(GetPickupCount)} ";

            if (pickupDef == null || !master)
                return 0;

            return 0;
        }

        public static void TryDeductFrom(this PickupDef pickupDef, CharacterMaster master, int count = 1)
        {
            const string LOG_PREFIX = $"{nameof(PickupExtensions)}.{nameof(TryDeductFrom)} ";

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
                            Log.Warning(LOG_PREFIX + $"{nameof(MiscPickupDef)} {miscPickupDef} is not implemented");
                        }
                    }
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"pickup {pickupDef} not implemented");
            }
        }
    }
}
