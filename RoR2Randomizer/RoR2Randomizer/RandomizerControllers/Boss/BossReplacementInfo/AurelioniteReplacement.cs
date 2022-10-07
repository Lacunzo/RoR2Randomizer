using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public sealed class AurelioniteReplacement : BaseBossReplacement
    {
        protected override BossReplacementType replacementType => BossReplacementType.Aurelionite;

        protected override CharacterMaster originalMasterPrefab => Caches.MasterPrefabs["TitanGoldMaster"];

        protected override bool replaceBossDropEvenIfExisting => true;

        protected override void bodyResolved()
        {
            base.bodyResolved();

#if DEBUG
            Log.Debug($"{nameof(AurelioniteReplacement)} {nameof(bodyResolved)}");
#endif

            setBodySubtitleIfNull("TITANGOLD_BODY_SUBTITLE");
        }
    }
}
