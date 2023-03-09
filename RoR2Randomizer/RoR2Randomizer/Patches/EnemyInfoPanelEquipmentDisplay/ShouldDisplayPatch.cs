using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.UI;
using System.Linq;

namespace RoR2Randomizer.Patches.EnemyInfoPanelEquipmentDisplay
{
    [PatchClass]
    static class ShouldDisplayPatch
    {
        static void Apply()
        {
            IL.RoR2.UI.EnemyInfoPanel.SetDisplayDataForViewer += EnemyInfoPanel_SetDisplayDataForViewer;
        }

        static void Cleanup()
        {
            IL.RoR2.UI.EnemyInfoPanel.SetDisplayDataForViewer -= EnemyInfoPanel_SetDisplayDataForViewer;
        }

        static void EnemyInfoPanel_SetDisplayDataForViewer(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            const int SHOULD_DISPLAY_LOCAL_INDEX = 0;

            // Just assume shouldDisplay is first local, not ideal, but it works for now and is unlikely to change
            if (c.TryGotoNext(MoveType.After, x => x.MatchStloc(SHOULD_DISPLAY_LOCAL_INDEX)))
            {
                c.Emit(OpCodes.Ldloca, SHOULD_DISPLAY_LOCAL_INDEX);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((ref bool shouldDisplay, HUD hud) =>
                {
                    shouldDisplay |= TrySetEquipmentsPatch.GetAllInventoryProviderEquipments(hud.targetMaster.teamIndex).Any();
                });
            }
            else
            {
                Log.Error("Failed to find patch location");
            }
        }
    }
}
