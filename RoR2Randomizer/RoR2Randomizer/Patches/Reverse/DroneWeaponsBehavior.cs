using HarmonyLib;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.Reverse
{
    [HarmonyPatch(typeof(RoR2.DroneWeaponsBehavior))]
    public static class DroneWeaponsBehavior
    {
        public static void SetNumDroneParts(Inventory inventory, int numParts)
        {
            setNumDroneParts(numParts, inventory, CharacterBody.BodyFlags.Mechanical, numParts);
        }

        [HarmonyPatch(nameof(RoR2.DroneWeaponsBehavior.UpdateMinionInventory))]
        [HarmonyReversePatch]
        static void setNumDroneParts(int numParts, Inventory inventory, CharacterBody.BodyFlags bodyFlags, int newStack)
        {
            void Manipulator(ILContext il)
            {
                ILCursor c = new ILCursor(il);

                FieldInfo DroneWeaponsBehavior_stack_FI = AccessTools.DeclaredField(typeof(CharacterBody.ItemBehavior), nameof(CharacterBody.ItemBehavior.stack));
                while (c.TryGotoNext(x => x.MatchLdarg(0),
                                     x => x.MatchLdfld(DroneWeaponsBehavior_stack_FI)))
                {
                    c.Index++; // Move before ldfld

                    // Remove ldfld, arg0 is the numParts argument
                    c.Remove();
                }
            }

            Manipulator(default);
        }
    }
}
