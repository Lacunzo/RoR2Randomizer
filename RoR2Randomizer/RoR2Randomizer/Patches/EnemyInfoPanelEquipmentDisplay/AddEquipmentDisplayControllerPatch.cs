using RoR2.UI;
using RoR2Randomizer.Utility.EnemyInfoEquipmentDisplay;

namespace RoR2Randomizer.Patches.EnemyInfoPanelEquipmentDisplay
{
    [PatchClass]
    static class AddEquipmentDisplayControllerPatch
    {
        static void Apply()
        {
            On.RoR2.UI.EnemyInfoPanel.Awake += EnemyInfoPanel_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.UI.EnemyInfoPanel.Awake -= EnemyInfoPanel_Awake;
        }

        static void EnemyInfoPanel_Awake(On.RoR2.UI.EnemyInfoPanel.orig_Awake orig, EnemyInfoPanel self)
        {
            orig(self);
            self.gameObject.AddComponent<EnemyInfoPanelEquipmentDisplayController>();
        }
    }
}
