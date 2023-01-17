using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.BossRandomizer
{
    public class SyncBossReplacementCharacter : SyncGameObjectReference
    {
        public delegate void OnReceivedDelegate(GameObject masterObject, BossReplacementType replacementType, MasterCatalog.MasterIndex? originalMasterIndex);
        public static event OnReceivedDelegate OnReceive;

        BossReplacementType _replacementType;
        MasterCatalog.MasterIndex? _originalMasterIndex;

        public SyncBossReplacementCharacter()
        {
        }

        public SyncBossReplacementCharacter(GameObject masterObject, BossReplacementType replacementType, MasterCatalog.MasterIndex? originalMasterIndex) : base(masterObject)
        {
            _replacementType = replacementType;
            if (!replacementType.IsValid())
            {
                Log.Warning($"about to sync a boss replacement with an invalid boss type of {replacementType}! {nameof(masterObject)}={masterObject}");
            }

            _originalMasterIndex = originalMasterIndex;
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WritePackedUInt32((uint)_replacementType);
            writer.WriteNullableNetworkMasterIndex(_originalMasterIndex);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _replacementType = (BossReplacementType)reader.ReadPackedUInt32();
            _originalMasterIndex = reader.ReadNullableNetworkMasterIndex();
        }

        protected override bool shouldHandleEvent => !NetworkServer.active && NetworkClient.active;

        protected override void onReceivedObjectResolved(GameObject obj)
        {
#if DEBUG
            Log.Debug($"{nameof(obj)}={obj}, {nameof(_replacementType)}={_replacementType}");
#endif

            OnReceive?.Invoke(obj, _replacementType, _originalMasterIndex);
        }
    }
}
