﻿#if !DISABLE_SKILL_RANDOMIZER
using RoR2;
using RoR2.Skills;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.Skill;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches
{
    [PatchClass]
    public static class SkillPatcher
    {
        static void Apply()
        {
            On.RoR2.GenericSkill.Awake += GenericSkill_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.GenericSkill.Awake -= GenericSkill_Awake;
        }

        static void GenericSkill_Awake(On.RoR2.GenericSkill.orig_Awake orig, GenericSkill self)
        {
            if (self && self.skillFamily)
            {
                CharacterBody body = self.GetComponent<CharacterBody>();
                if (body)
                {
                    SkillRandomizerController.RandomizeSkill(self, body);
                }
            }

            orig(self);
        }
    }
}
#endif