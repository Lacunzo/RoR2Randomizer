using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public abstract class BaseMithrixReplacement : BaseBossReplacement
    {
        protected override void bodyResolved()
        {
            base.bodyResolved();

            if (NetworkServer.active)
            {
                if ((Caches.Bodies.VoidlingBaseBodyIndex != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingBaseBodyIndex) || 
                    (Caches.Bodies.VoidlingPhase1BodyIndex != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase1BodyIndex) ||
                    (Caches.Bodies.VoidlingPhase2BodyIndex != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase2BodyIndex) ||
                    (Caches.Bodies.VoidlingPhase3BodyIndex != BodyIndex.None && _body.bodyIndex == Caches.Bodies.VoidlingPhase3BodyIndex))
                {
                    TeleportHelper.TeleportBody(_body, _body.transform.position + new Vector3(0f, 25f, 0f));
                }
            }
        }
    }
}
