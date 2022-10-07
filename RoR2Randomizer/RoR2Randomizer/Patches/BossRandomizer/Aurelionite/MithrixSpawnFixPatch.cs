using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers;

namespace RoR2Randomizer.Patches.BossRandomizer.Aurelionite
{
    [PatchClass]
    static class MithrixSpawnFixPatch
    {
        static readonly ILContext.Manipulator _replaceMithrixHurtReference = CharacterReplacements.FixMasterIndexReferences(() => ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeMithrix, nameof(GoldTitanManager.brotherHurtMasterIndex));

        static void Apply()
        {
            IL.RoR2.GoldTitanManager.OnBossGroupStartServer += _replaceMithrixHurtReference;
        }

        static void Cleanup()
        {
            IL.RoR2.GoldTitanManager.OnBossGroupStartServer -= _replaceMithrixHurtReference;
        }
    }
}
