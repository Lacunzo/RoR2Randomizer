using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Networking.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    public class ExplicitSpawnReplacementInfo : CharacterReplacementInfo
    {
        MasterCatalog.MasterIndex _originalMasterIndex;
        public MasterCatalog.MasterIndex OriginalMasterIndex
        {
            get
            {
                return _originalMasterIndex;
            }
            set
            {
                _originalMasterIndex = value;

                GameObject prefabObj = MasterCatalog.GetMasterPrefab(value);
                if (!prefabObj || !prefabObj.TryGetComponent<CharacterMaster>(out _cachedMasterPrefab))
                    _cachedMasterPrefab = null;
            }
        }

        CharacterMaster _cachedMasterPrefab;
        protected override CharacterMaster originalMasterPrefab => _cachedMasterPrefab;

        protected override bool isNetworked => true;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
            yield return new SyncExplicitSpawnReplacement(_master.gameObject, _originalMasterIndex);
        }
    }
}
