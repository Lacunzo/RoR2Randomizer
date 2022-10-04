using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public sealed class AlloyWorshipUnitReplacement : BaseBossReplacement
    {
        protected override BossReplacementType replacementType => BossReplacementType.AlloyWorshipUnit;

        protected override CharacterMaster originalBossMasterPrefab => Caches.MasterPrefabs["SuperRoboBallBossMaster"];

        protected override void bodyResolved()
        {
            base.bodyResolved();

            setBodySubtitleIfNull("SUPERROBOBALLBOSS_BODY_SUBTITLE");
        }
    }
}
