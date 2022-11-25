using MonoMod.Cil;
using RoR2;
using RoR2.CharacterSpeech;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    [PatchClass]
    static class BrotherSpeechDriver_ReplaceHeretic
    {
        static void Apply()
        {
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.DoInitialSightResponse += replaceHereticIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnBodyKill += replaceHereticIndexPatch;
        }

        static void Cleanup()
        {
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.DoInitialSightResponse -= replaceHereticIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnBodyKill -= replaceHereticIndexPatch;
        }

        static void replaceHereticIndexPatch(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchLdsfld<BrotherSpeechDriver>(nameof(BrotherSpeechDriver.hereticBodyIndex))))
            {
                c.Index++;
                c.EmitDelegate(static (BodyIndex index) =>
                {
                    if (ConfigManager.ExplicitSpawnRandomizer.RandomizeHeretic && ExplicitSpawnRandomizerController.TryGetReplacementBodyIndex(index, out BodyIndex hereticReplacementIndex))
                    {
                        return hereticReplacementIndex;
                    }
                    else
                    {
                        return index;
                    }
                });
            }
        }
    }
}
