using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterSpeech;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.Boss;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            ILCursor c = new ILCursor(il);
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
            }
        }
    }
}
