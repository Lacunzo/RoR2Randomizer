using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public sealed class LunarScavReplacement : BaseBossReplacement
    {
        public uint LunarScavIndex;

        protected override BossReplacementType replacementType => BossReplacementType.LunarScav1 + LunarScavIndex;

        protected override CharacterMaster originalBossMasterPrefab => Caches.MasterPrefabs[$"ScavLunar{LunarScavIndex + 1}Master"];

        protected override void bodyResolved()
        {
            base.bodyResolved();

            if (string.IsNullOrEmpty(_body.subtitleNameToken))
            {
                setBodySubtitle("SCAVLUNAR_BODY_SUBTITLE");
            }
        }
    }
}
