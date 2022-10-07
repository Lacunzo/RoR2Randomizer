using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System;

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
    }
}
