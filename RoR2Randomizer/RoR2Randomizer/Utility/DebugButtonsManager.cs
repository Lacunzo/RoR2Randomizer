#if DEBUG
using RoR2;
using RoR2Randomizer.Patches.Debug;
using RoR2Randomizer.RandomizerControllers;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    [RandomizerController]
    public class DebugButtonsManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                SpawnDisabler.ToggleSpawnsDisabled();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Stage.instance.BeginAdvanceStage(Run.instance.nextStageScene);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.SIRENS_CALL_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                Vector3 pos;
                Quaternion rot;
                if (PlayerCharacterMasterController.instances.Count > 0)
                {
                    Transform modelTransform = PlayerCharacterMasterController.instances[0].master.GetBody().modelLocator.modelTransform;
                    pos = modelTransform.position;
                    rot = modelTransform.rotation;
                }
                else
                {
                    pos = Vector3.zero;
                    rot = Quaternion.identity;
                }

                GameObject beetleObj = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/BeetleQueenMaster"), pos, rot);
                CharacterMaster beetleMaster = beetleObj.GetComponent<CharacterMaster>();
                if (beetleMaster)
                {
                    beetleMaster.teamIndex = TeamIndex.Monster;
                    NetworkServer.Spawn(beetleObj);
                    beetleMaster.SpawnBodyHere();

                    //beetleMaster.inventory.SetEquipmentIndex(RoR2Content.Elites.Poison.eliteEquipmentDef.equipmentIndex);
                    //beetleMaster.inventory.SetEquipmentIndex(DLC1Content.Elites.Earth.eliteEquipmentDef.equipmentIndex);
                    //beetleMaster.inventory.SetEquipmentIndex(DLC1Content.Elites.Void.eliteEquipmentDef.equipmentIndex);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                foreach (PlayerCharacterMasterController playerMasterController in PlayerCharacterMasterController.instances)
                {
                    Inventory inventory = playerMasterController.master.inventory;

                    inventory.GiveRandomItems(100, false, false);
                    //inventory.SetEquipmentIndex(RoR2Content.Equipment.Scanner.equipmentIndex);

                    // inventory.GiveItem(RoR2Content.Items.RoboBallBuddy);
                    //inventory.GiveItem(DLC1Content.Items.DroneWeapons);

                    //inventory.SetEquipmentIndex(RoR2Content.Equipment.DroneBackup.equipmentIndex);

                    //inventory.GiveItem(RoR2Content.Items.BleedOnHit, 10);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.COMMENCEMENT_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.GOLD_SHORES_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                foreach (PlayerCharacterMasterController masterController in PlayerCharacterMasterController.instances)
                {
                    masterController.master.money += 1000;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.ABANDONED_AQUEDUCT_SCENE_NAME));
            }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName(Constants.SceneNames.VOID_FIELDS_SCENE_NAME));
            }
        }
    }
}
#endif