using R2API;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;

namespace RoR2Randomizer.Utility.EnemyInfoEquipmentDisplay
{
    [RequireComponent(typeof(EnemyInfoPanel))]
    public class EnemyInfoPanelEquipmentDisplayController : MonoBehaviour
    {
        EnemyInfoPanel _enemyInfoPanel;

        GameObject _equipmentContainer;
        EquipmentInventoryDisplay _equipmentDisplay;

        EquipmentIndex[] _lastEquipmentIndices = Array.Empty<EquipmentIndex>();

        const string EQUIPMENT_LABEL_TOKEN = "ARENA_HUD_MONSTER_EQUIPMENTS_LABEL";

        [SystemInitializer]
        static void Init()
        {
            LanguageAPI.Add(EQUIPMENT_LABEL_TOKEN, "Monsters' Equipment");
        }

        void Awake()
        {
            _enemyInfoPanel = GetComponent<EnemyInfoPanel>();
            initializeEquipmentContainer();
        }

        void initializeEquipmentContainer()
        {
            _equipmentContainer = Instantiate(_enemyInfoPanel.inventoryContainer, _enemyInfoPanel.inventoryContainer.transform.parent);
            _equipmentContainer.name = "EquipmentContainer";

            Transform equipmentLabel = _equipmentContainer.transform.Find("InventoryLabel");
            equipmentLabel.name = "EquipmentLabel";
            equipmentLabel.GetComponent<LanguageTextMeshController>().token = EQUIPMENT_LABEL_TOKEN;

            Transform equipmentDisplay = _equipmentContainer.transform.Find("InventoryDisplay");
            equipmentDisplay.name = "EquipmentDisplay";
            Destroy(equipmentDisplay.GetComponent<ItemInventoryDisplay>());
            _equipmentDisplay = equipmentDisplay.gameObject.AddComponent<EquipmentInventoryDisplay>();

            _equipmentContainer.SetActive(false);
        }

        public void TrySetEquipments(EquipmentIndex[] equipments)
        {
            if (!MiscUtils.ArrayEqual(_lastEquipmentIndices, equipments))
            {
                _equipmentContainer.SetActive(equipments.Length > 0);

                _lastEquipmentIndices = equipments;
                _equipmentDisplay.SetEquipments(equipments);
            }
        }
    }
}
