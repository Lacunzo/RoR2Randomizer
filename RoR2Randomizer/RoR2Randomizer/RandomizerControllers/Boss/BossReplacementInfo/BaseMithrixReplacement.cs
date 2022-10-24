using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public abstract class BaseMithrixReplacement : BaseBossReplacement
    {
        protected override CharacterMaster originalMasterPrefab => replacementType switch
        {
            BossReplacementType.MithrixNormal => Caches.MasterPrefabs["BrotherMaster"],
            BossReplacementType.MithrixHurt => Caches.MasterPrefabs["BrotherHurtMaster"],
            _ => null
        };

        protected override void bodyResolved()
        {
            base.bodyResolved();

            if (NetworkServer.active)
            {
                if ((Caches.Bodies.VoidlingPhase1 != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase1) ||
                    (Caches.Bodies.VoidlingPhase2 != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase2) ||
                    (Caches.Bodies.VoidlingPhase3 != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase3))
                {
                    TeleportHelper.TeleportBody(_body, _body.transform.position + new Vector3(0f, 25f, 0f));
                }
            }
        }
    }
}
