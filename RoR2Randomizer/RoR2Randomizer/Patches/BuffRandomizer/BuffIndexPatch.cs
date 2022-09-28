using RoR2;
using RoR2Randomizer.RandomizerController.Buff;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    public static class BuffIndexPatch
    {
        public static void Apply()
        {
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
        }

        public static void Cleanup()
        {
            On.RoR2.CharacterBody.SetBuffCount -= CharacterBody_SetBuffCount;
        }

        static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            BuffRandomizerController.TryReplaceBuffIndex(ref buffType);

            orig(self, buffType, newCount);
        }
    }
}
