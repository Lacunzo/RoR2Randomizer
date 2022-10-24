using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    static class TimedBuffFixPatch
    {
        static void Apply()
        {
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBody_UpdateBuffs;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterBody.UpdateBuffs -= CharacterBody_UpdateBuffs;
        }

        static void CharacterBody_UpdateBuffs(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self, float deltaTime)
        {
            BuffIndexPatch.SkipPatchCount++;
            GetBuffIndex_BuffIndex_ReplacePatch.ForceDisable = true;

            orig(self, deltaTime);

            GetBuffIndex_BuffIndex_ReplacePatch.ForceDisable = false;
            BuffIndexPatch.SkipPatchCount--;
        }
    }
}
