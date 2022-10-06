using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerController.ExplicitSpawn
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

        protected override void initializeServer()
        {
            base.initializeServer();

            new SyncExplicitSpawnReplacement(_master.gameObject, _originalMasterIndex).Send(NetworkDestination.Clients);
        }
    }
}
