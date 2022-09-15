using EntityStates;
using HarmonyLib;
using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Bandit2.Weapon
{
    public static class SlashBlade
    {
        public static void Apply()
        {
            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter += SlashBlade_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter -= SlashBlade_OnEnter;
        }

        static void SlashBlade_OnEnter(On.EntityStates.Bandit2.Weapon.SlashBlade.orig_OnEnter orig, global::EntityStates.Bandit2.Weapon.SlashBlade self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, "BladeMesh", CustomChildTransformManager.ChildFlags.ForceNewObject);

            orig(self);
        }
    }
}
