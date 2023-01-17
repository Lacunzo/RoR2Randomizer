using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterSpeech;
using RoR2Randomizer.RandomizerControllers.Boss;
using System.Reflection;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    [PatchClass]
    static class BrotherSpeechDriver_ReplaceAurelionite
    {
        static void Apply()
        {
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.DoInitialSightResponse += replaceAurelioniteIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnBodyKill += replaceAurelioniteIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnCharacterBodyStartGlobal += replaceAurelioniteIndexPatch;
        }

        static void Cleanup()
        {
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.DoInitialSightResponse -= replaceAurelioniteIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnBodyKill -= replaceAurelioniteIndexPatch;
            IL.RoR2.CharacterSpeech.BrotherSpeechDriver.OnCharacterBodyStartGlobal -= replaceAurelioniteIndexPatch;
        }

        static readonly FieldInfo BrotherSpeechDriver_titanGoldBodyIndex_FI = AccessTools.DeclaredField(typeof(BrotherSpeechDriver), nameof(BrotherSpeechDriver.titanGoldBodyIndex));

        static void replaceAurelioniteIndexPatch(ILContext il)
        {
            string LOG_PREFIX = $"({il?.Method?.FullName ?? "null"}) ";

            ILCursor c = new ILCursor(il);

            int patchCount = 0;
            while (c.TryGotoNext(x => x.MatchLdsfld(BrotherSpeechDriver_titanGoldBodyIndex_FI)))
            {
                c.Index++;
                c.EmitDelegate((BodyIndex index) =>
                {
                    if (BossRandomizerController.Aurelionite.TryGetAurelioniteMasterReplacementBodyIndex(out BodyIndex replacementIndex))
                    {
                        return replacementIndex;
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
