using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public sealed class LunarScavReplacement : BaseBossReplacement
    {
        public uint LunarScavIndex;

        protected override BossReplacementType replacementType => BossReplacementType.LunarScav1 + LunarScavIndex;

        protected override CharacterMaster originalMasterPrefab => Caches.MasterPrefabs[$"ScavLunar{LunarScavIndex + 1}Master"];
    }
}
