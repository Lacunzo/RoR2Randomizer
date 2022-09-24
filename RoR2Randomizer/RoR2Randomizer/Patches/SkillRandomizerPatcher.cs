using RoR2;
using RoR2.Skills;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.Skill;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches
{
    public static class SkillRandomizerPatcher
    {
        public static void Apply()
        {
            On.RoR2.GenericSkill.Awake += GenericSkill_Awake;

            Fixes.Skills.EntityStates.AcidLarva.LarvaLeap.Apply();

            Fixes.Skills.EntityStates.AncientWispMonster.ChannelRain.Apply();
            Fixes.Skills.EntityStates.AncientWispMonster.Enrage.Apply();

            Fixes.Skills.EntityStates.ArtifactShell.FireSolarFlares.Apply();

            Fixes.Skills.EntityStates.Assassin2.DashStrike.Apply();
            Fixes.Skills.EntityStates.Assassin2.ThrowShuriken.Apply();

            Fixes.Skills.EntityStates.Bandit2.Weapon.Reload.Apply();
            Fixes.Skills.EntityStates.Bandit2.Weapon.SlashBlade.Apply();

            Fixes.Skills.EntityStates.BrotherMonster.SpellBaseState.Apply();
            Fixes.Skills.EntityStates.BrotherMonster.SpellChannelState.Apply();

            Fixes.Skills.EntityStates.Drone.DroneWeapon.FireTurret.Apply();

            Fixes.Skills.EntityStates.GreaterWispMonster.ChargeCannons.Apply();

            Fixes.Skills.EntityStates.NewtMonster.KickFromShop.Apply();

            Fixes.Skills.EntityStates.VoidRaidCrab.EscapeDeath.Apply();
        }

        public static void Cleanup()
        {
            On.RoR2.GenericSkill.Awake -= GenericSkill_Awake;

            Fixes.Skills.EntityStates.AcidLarva.LarvaLeap.Cleanup();

            Fixes.Skills.EntityStates.AncientWispMonster.ChannelRain.Cleanup();
            Fixes.Skills.EntityStates.AncientWispMonster.Enrage.Cleanup();

            Fixes.Skills.EntityStates.ArtifactShell.FireSolarFlares.Cleanup();

            Fixes.Skills.EntityStates.Assassin2.DashStrike.Cleanup();
            Fixes.Skills.EntityStates.Assassin2.ThrowShuriken.Cleanup();

            Fixes.Skills.EntityStates.Bandit2.Weapon.Reload.Cleanup();
            Fixes.Skills.EntityStates.Bandit2.Weapon.SlashBlade.Cleanup();

            Fixes.Skills.EntityStates.BrotherMonster.SpellBaseState.Cleanup();
            Fixes.Skills.EntityStates.BrotherMonster.SpellChannelState.Cleanup();

            Fixes.Skills.EntityStates.Drone.DroneWeapon.FireTurret.Cleanup();

            Fixes.Skills.EntityStates.GreaterWispMonster.ChargeCannons.Cleanup();

            Fixes.Skills.EntityStates.NewtMonster.KickFromShop.Cleanup();

            Fixes.Skills.EntityStates.VoidRaidCrab.EscapeDeath.Cleanup();
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
