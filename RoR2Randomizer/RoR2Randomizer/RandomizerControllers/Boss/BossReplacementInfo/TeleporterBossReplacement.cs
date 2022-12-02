using HG;
using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public sealed class TeleporterBossReplacement : BaseBossReplacement
    {
        protected override BossReplacementType replacementType => BossReplacementType.TeleporterBoss;

        protected override SetSubtitleMode subtitleOverrideMode => SetSubtitleMode.DontOverrideIfBothNotNull;
    }
}
