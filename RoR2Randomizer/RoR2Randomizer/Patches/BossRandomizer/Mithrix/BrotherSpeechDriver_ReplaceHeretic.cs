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
            string LOG_PREFIX = $"{nameof(BrotherSpeechDriver_ReplaceHeretic)}.{nameof(replaceHereticIndexPatch)} ({il?.Method?.FullName ?? "null"}) ";

            ILCursor c = new ILCursor(il);

            int patchCount = 0;
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

                patchCount++;
            }

            if (patchCount > 0)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"patched {patchCount} locations");
#endif
            }
            else
            {
                Log.Warning(LOG_PREFIX + "patched 0 locations");
            }
        }
    }
}
