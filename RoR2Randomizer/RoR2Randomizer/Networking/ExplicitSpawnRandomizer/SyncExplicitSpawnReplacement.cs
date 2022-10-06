using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ExplicitSpawnRandomizer
{
    public sealed class SyncExplicitSpawnReplacement : SyncGameObjectReference
    {
        public delegate void OnReceivedDelegate(GameObject masterObject, MasterCatalog.MasterIndex originalMasterIndex);
        public static event OnReceivedDelegate OnReceive;

        MasterCatalog.MasterIndex _originalMasterIndex;

        public SyncExplicitSpawnReplacement()
        {
        }

        public SyncExplicitSpawnReplacement(GameObject obj, MasterCatalog.MasterIndex originalMasterIndex) : base(obj)
        {
            _originalMasterIndex = originalMasterIndex;
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            GeneratedNetworkCode._WriteNetworkMasterIndex_MasterCatalog(writer, _originalMasterIndex);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _originalMasterIndex = GeneratedNetworkCode._ReadNetworkMasterIndex_MasterCatalog(reader);
        }

        protected override bool shouldHandleEvent => !NetworkServer.active && NetworkClient.active;

        protected override void onReceivedObjectResolved(GameObject obj)
        {
            OnReceive?.Invoke(obj, _originalMasterIndex);
        }
    }
}
