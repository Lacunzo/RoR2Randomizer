using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class Shopkeeper_SpawnHook
    {
        static MasterCatalog.MasterIndex _shopkeeperMasterIndex = MasterCatalog.MasterIndex.none;
        static SceneIndex _newtShopSceneIndex = SceneIndex.Invalid;

        [SystemInitializer(typeof(SceneCatalog), typeof(MasterCatalog))]
        static void Init()
        {
            _newtShopSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.NEWT_SHOP_SCENE_NAME);

            _shopkeeperMasterIndex = MasterCatalog.FindMasterIndex("ShopkeeperMaster");
        }

        static void Apply()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        static void Cleanup()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        static void onSceneLoaded(SceneDef scene)
        {
            if (_newtShopSceneIndex != SceneIndex.Invalid && _shopkeeperMasterIndex.isValid && NetworkServer.active && scene.sceneDefIndex == _newtShopSceneIndex)
            {
                MasterCatalog.MasterIndex replacementIndex = ExplicitSpawnRandomizerController.GetSummonReplacement(_shopkeeperMasterIndex);
                if (replacementIndex.isValid)
                {
                    GameObject replacementPrefabObj = MasterCatalog.GetMasterPrefab(replacementIndex);
                    if (replacementPrefabObj && replacementPrefabObj.TryGetComponent<CharacterMaster>(out CharacterMaster replacementPrefab) && replacementPrefab.bodyPrefab)
                    {
                        const string STORE_HOLDER_OBJECT_NAME = "HOLDER: Store";
                        GameObject storeHolder = GameObject.Find(STORE_HOLDER_OBJECT_NAME);
                        if (storeHolder)
                        {
                            Transform shopkeeperMasterTransform = storeHolder.transform.Find("HOLDER: Store Platforms/ShopkeeperPosition/ShopkeeperMaster");
                            if (shopkeeperMasterTransform && shopkeeperMasterTransform.TryGetComponent<CharacterMaster>(out CharacterMaster shopKeeperMaster))
                            {
                                shopKeeperMaster.bodyPrefab = replacementPrefab.bodyPrefab;

#if DEBUG
                                Log.Debug("Replaced Shopkeeper");
#endif
                            }
                        }
                        else
                        {
                            Log.Warning($"{nameof(Shopkeeper_SpawnHook)}.{nameof(onSceneLoaded)} could not find {STORE_HOLDER_OBJECT_NAME}");
                        }
                    }
                }
            }
        }
    }
}
