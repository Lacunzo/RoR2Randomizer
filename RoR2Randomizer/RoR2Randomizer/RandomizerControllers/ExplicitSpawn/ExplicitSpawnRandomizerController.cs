using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    [RandomizerController]
    public sealed class ExplicitSpawnRandomizerController : BaseRandomizerController
    {
        [SystemInitializer(typeof(Caches.Bodies))]
        static void Init()
        {
            if (Caches.Bodies.SquidTurretBodyIndex != BodyIndex.None)
                ItemDescriptionNameReplacementManager.AddEntry("ITEM_SQUIDTURRET_PICKUP", Caches.Bodies.SquidTurretBodyIndex);

            if (Caches.Bodies.MinorConstructOnKillBodyIndex != BodyIndex.None)
                ItemDescriptionNameReplacementManager.AddEntry("ITEM_MINORCONSTRUCTONKILL_PICKUP", Caches.Bodies.MinorConstructOnKillBodyIndex);
        }

        static readonly RunSpecific<bool> _isEnabledServer = new RunSpecific<bool>();

        public override bool IsRandomizerEnabled => IsActive;
        public static bool IsActive => (NetworkServer.active && (ConfigManager.ExplicitSpawnRandomizer.Enabled || CharacterReplacements.IsAnyForcedCharacterModeEnabled)) || (NetworkClient.active && _isEnabledServer);

        protected override bool isNetworked => true;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
            yield return new SyncExplicitSpawnRandomizerEnabled(IsActive);
        }

        public static MasterCatalog.MasterIndex GetOriginalMasterIndex(GameObject replacementObject)
        {
            if (IsActive)
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
            if (IsActive)
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
            if (IsActive)
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
            if (IsActive)
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
            if (IsActive)
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

        public static bool TryGetReplacementBodyName(string originalName, out string replacementName)
        {
            if (IsActive)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(originalName);
                if (index != BodyIndex.None && TryGetReplacementBodyIndex(index, out BodyIndex replacementIndex))
                {
                    GameObject replacementPrefab = BodyCatalog.GetBodyPrefab(replacementIndex);
                    if (replacementPrefab)
                    {
                        replacementName = replacementPrefab.name;
                        return true;
                    }
                }
            }

            replacementName = null;
            return false;
        }

        public static bool TryGetReplacementBodyIndex(BodyIndex original, out BodyIndex replacement)
        {
            if (IsActive)
            {
                return (replacement = CharacterReplacements.GetReplacementBodyIndex(original)) != BodyIndex.None;
            }
            else
            {
                replacement = BodyIndex.None;
                return false;
            }
        }

        public static bool TryGetOriginalBodyName(string replacementName, out string originalName)
        {
            if (IsActive)
            {
                BodyIndex index = BodyCatalog.FindBodyIndex(replacementName);
                if (index != BodyIndex.None)
                {
                    BodyIndex originalIndex = CharacterReplacements.GetOriginalBodyIndex(index);
                    if (originalIndex != BodyIndex.None)
                    {
                        GameObject originalPrefab = BodyCatalog.GetBodyPrefab(originalIndex);
                        if (originalPrefab)
                        {
                            originalName = originalPrefab.name;
                            return true;
                        }
                    }
                }
            }

            originalName = null;
            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            SyncExplicitSpawnReplacement.OnReceive += RegisterSpawnedReplacement;
            SyncExplicitSpawnRandomizerEnabled.OnReceive += SyncExplicitSpawnRandomizerEnabled_OnReceive;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SyncExplicitSpawnReplacement.OnReceive -= RegisterSpawnedReplacement;
            SyncExplicitSpawnRandomizerEnabled.OnReceive -= SyncExplicitSpawnRandomizerEnabled_OnReceive;
        }

        static void SyncExplicitSpawnRandomizerEnabled_OnReceive(bool isEnabled)
        {
            _isEnabledServer.Value = isEnabled;
        }

        public static void RegisterSpawnedReplacement(GameObject masterObject, MasterCatalog.MasterIndex originalMasterIndex)
        {
            if ((!NetworkServer.active || IsActive) && masterObject && originalMasterIndex.isValid)
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
