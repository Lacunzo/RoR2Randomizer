using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerController.ExplicitSpawn
{
    public sealed class ExplicitSpawnRandomizerController : Singleton<ExplicitSpawnRandomizerController>
    {
        public static MasterCatalog.MasterIndex GetOriginalMasterIndex(GameObject replacementObject)
        {
            if (ConfigManager.ExplicitSpawnRandomizer.Enabled)
            {
                if (replacementObject && replacementObject.TryGetComponent<CharacterMaster>(out CharacterMaster master) && master.masterIndex.isValid)
                {
                    return GetOriginalMasterIndex(master.masterIndex);
                }
            }

            return MasterCatalog.MasterIndex.none;
        }

        public static MasterCatalog.MasterIndex GetOriginalMasterIndex(MasterCatalog.MasterIndex replacement)
        {
            if (ConfigManager.ExplicitSpawnRandomizer.Enabled)
            {
                return CharacterReplacements.GetOriginalMasterIndex(replacement);
            }
            else
            {
                return MasterCatalog.MasterIndex.none;
            }
        }

        public static bool TryReplaceSummon(ref GameObject prefab, out GameObject originalPrefab)
        {
            originalPrefab = prefab;
            return TryReplaceSummon(ref prefab);
        }

        public static bool TryReplaceSummon(ref GameObject prefab)
        {
            if (ConfigManager.ExplicitSpawnRandomizer.Enabled)
            {
                return CharacterReplacements.TryReplaceMasterPrefab(ref prefab);
            }
            else
            {
                return false;
            }
        }

        public static GameObject GetSummonReplacement(GameObject original)
        {
            TryReplaceSummon(ref original);
            return original;
        }

        public static CharacterMaster GetSummonReplacement(CharacterMaster original)
        {
            GameObject replacementMasterPrefabObject = MasterCatalog.GetMasterPrefab(GetSummonReplacement(original.masterIndex));
            if (replacementMasterPrefabObject)
            {
                return replacementMasterPrefabObject.GetComponent<CharacterMaster>();
            }
            else
            {
                return null;
            }
        }

        public static MasterCatalog.MasterIndex GetSummonReplacement(MasterCatalog.MasterIndex original)
        {
            if (ConfigManager.ExplicitSpawnRandomizer.Enabled)
            {
                return CharacterReplacements.GetReplacementForMasterIndex(original);
            }
            else
            {
                return MasterCatalog.MasterIndex.none;
            }
        }

        public static bool TryGetReplacementMaster(CharacterMaster originalPrefab, out CharacterMaster replacementPrefab)
        {
            if (ConfigManager.ExplicitSpawnRandomizer.Enabled)
            {
                GameObject replacementPrefabObject = CharacterReplacements.GetReplacementMasterPrefab(originalPrefab.name);
                if (replacementPrefabObject && replacementPrefabObject.TryGetComponent<CharacterMaster>(out replacementPrefab))
                {
                    return true;
                }
            }

            replacementPrefab = null;
            return false;
        }

        public static void ReplaceDirectorSpawnRequest(DirectorSpawnRequest spawnRequest)
        {
            if (TryReplaceSummon(ref spawnRequest.spawnCard.prefab, out GameObject originalPrefab))
            {
                MiscUtils.AppendDelegate(ref spawnRequest.onSpawnedServer, (SpawnCard.SpawnResult result) =>
                {
                    if (result.spawnRequest != null && result.spawnRequest.spawnCard == spawnRequest.spawnCard)
                    {
                        result.spawnRequest.spawnCard.prefab = originalPrefab;

                        if (result.success && result.spawnedInstance)
                        {
                            if (originalPrefab && originalPrefab.TryGetComponent<CharacterMaster>(out CharacterMaster originalMasterPrefab))
                            {
                                RegisterSpawnedReplacement(result.spawnedInstance, originalMasterPrefab.masterIndex);
                            }
                        }
                    }
                });
            }
        }

        protected override void Awake()
        {
            base.Awake();

            SyncExplicitSpawnReplacement.OnReceive += RegisterSpawnedReplacement;
        }

        void OnDestroy()
        {
            SyncExplicitSpawnReplacement.OnReceive -= RegisterSpawnedReplacement;
        }

        public static void RegisterSpawnedReplacement(GameObject masterObject, MasterCatalog.MasterIndex originalMasterIndex)
        {
            if ((!NetworkServer.active || ConfigManager.ExplicitSpawnRandomizer.Enabled) && masterObject && originalMasterIndex.isValid)
            {
                ExplicitSpawnReplacementInfo replacementInfo = masterObject.AddComponent<ExplicitSpawnReplacementInfo>();
                replacementInfo.OriginalMasterIndex = originalMasterIndex;
                replacementInfo.Initialize();
            }
        }

        public static void RegisterSpawnedReplacement(GameObject masterObject)
        {
            RegisterSpawnedReplacement(masterObject, GetOriginalMasterIndex(masterObject));
        }
    }
}
