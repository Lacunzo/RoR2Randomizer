using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public sealed class AurelioniteReplacement : BaseBossReplacement
    {
        protected override BossReplacementType replacementType => BossReplacementType.Aurelionite;

        protected override CharacterMaster originalMasterPrefab => Caches.MasterPrefabs["TitanGoldMaster"];

        protected override bool replaceBossDropEvenIfExisting => true;
    }
}
