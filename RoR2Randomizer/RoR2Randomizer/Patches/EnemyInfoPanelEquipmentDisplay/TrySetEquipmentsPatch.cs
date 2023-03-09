using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using RoR2Randomizer.Utility.EnemyInfoEquipmentDisplay;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Patches.EnemyInfoPanelEquipmentDisplay
{
    [PatchClass]
    static class TrySetEquipmentsPatch
    {
        static void Apply()
        {
            IL.RoR2.UI.EnemyInfoPanel.SetDisplayDataForViewer += EnemyInfoPanel_SetDisplayDataForViewer_TrySetEqipments;
        }

        static void Cleanup()
        {
            IL.RoR2.UI.EnemyInfoPanel.SetDisplayDataForViewer -= EnemyInfoPanel_SetDisplayDataForViewer_TrySetEqipments;
        }

        static void EnemyInfoPanel_SetDisplayDataForViewer_TrySetEqipments(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursor;

            int enemyInfoPanelLocalIndex = -1;
            if (c.TryFindNext(out foundCursor,
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => EnemyInfoPanel.SetDisplayingOnHud(default, default))),
                              x => x.MatchStloc(out enemyInfoPanelLocalIndex)))
            {
#if DEBUG
                Log.Debug($"{nameof(enemyInfoPanelLocalIndex)}={enemyInfoPanelLocalIndex}");
#endif

                if (c.TryGotoNext(MoveType.After,
                                  x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<EnemyInfoPanel>(_ => _.TrySetItems<ItemIndex[], int[]>(default, default, default)))))
                {
                    c.Emit(OpCodes.Ldloc, enemyInfoPanelLocalIndex);
                    c.EmitDelegate((EnemyInfoPanel enemyInfoPanel) =>
                    {
                        if (!enemyInfoPanel.TryGetComponent(out EnemyInfoPanelEquipmentDisplayController equipmentDisplayController))
                            return;

                        TeamIndex targetMasterTeamIndex = enemyInfoPanel.hud.targetMaster.teamIndex;

                        List<EnemyInfoPanelInventoryProvider> inventoryProviders = InstanceTracker.GetInstancesList<EnemyInfoPanelInventoryProvider>();

                        EquipmentIndex[] equipments = inventoryProviders.SelectMany(inventoryProvider =>
                        {
                            if (inventoryProvider.teamFilter.teamIndex == targetMasterTeamIndex)
                                return Enumerable.Empty<EquipmentIndex>();

                            Inventory inventory = inventoryProvider.inventory;
                            return Enumerable.Range(0, inventory.GetEquipmentSlotCount())
                                             .Select(i => inventory.GetEquipment((uint)i).equipmentIndex)
                                             .Where(static ei => ei != EquipmentIndex.None);
                        }).ToArray();

                        equipmentDisplayController.TrySetEquipments(equipments);
                    });
                }
                else
                {
                    Log.Error("Failed to find TrySetItems call");
                }
            }
            else
            {
                Log.Error("Failed to find enemyInfoPanel local index");
            }
        }
    }
}
