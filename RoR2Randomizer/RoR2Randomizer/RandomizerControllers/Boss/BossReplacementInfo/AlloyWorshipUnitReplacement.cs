using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public sealed class AlloyWorshipUnitReplacement : BaseBossReplacement
    {
        protected override BossReplacementType replacementType => BossReplacementType.AlloyWorshipUnit;

        protected override CharacterMaster originalMasterPrefab => Caches.MasterPrefabs["SuperRoboBallBossMaster"];
    }
}
