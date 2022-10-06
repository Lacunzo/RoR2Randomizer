using R2API.Networking.Interfaces;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.BossRandomizer
{
    public class SyncBossReplacementCharacter : SyncGameObjectReference
    {
        public delegate void OnReceivedDelegate(GameObject masterObject, BossReplacementType replacementType);
        public static event OnReceivedDelegate OnReceive;

        BossReplacementType _replacementType;

        public SyncBossReplacementCharacter()
        {
        }

        public SyncBossReplacementCharacter(GameObject masterObject, BossReplacementType replacementType) : base(masterObject)
        {
            _replacementType = replacementType;

            if (!replacementType.IsValid())
            {
                Log.Warning($"{nameof(SyncBossReplacementCharacter)} is about to sync a boss replacement with an invalid boss type of {replacementType}! {nameof(masterObject)}: {masterObject}");
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.WritePackedUInt32((uint)_replacementType);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _replacementType = (BossReplacementType)reader.ReadPackedUInt32();
        }

        protected override bool shouldHandleEvent => !NetworkServer.active && NetworkClient.active;

        protected override void onReceivedObjectResolved(GameObject obj)
        {
#if DEBUG
            Log.Debug($"{nameof(SyncBossReplacementCharacter)} object resolved: obj: {obj}, _replacementType: {_replacementType}");
#endif

            OnReceive?.Invoke(obj, _replacementType);
        }
    }
}
