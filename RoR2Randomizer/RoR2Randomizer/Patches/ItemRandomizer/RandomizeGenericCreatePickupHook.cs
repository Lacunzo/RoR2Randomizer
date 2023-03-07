#if !DISABLE_ITEM_RANDOMIZER
using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using static RoR2Randomizer.Patches.ItemRandomizer.PickupDropletController_PickupRandomizeHook;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class RandomizeGenericCreatePickupHook
    {
        static void Apply()
        {
            // Scrap
            IL.EntityStates.Scrapper.ScrappingToIdle.OnEnter += GenericEnablePatchHook;

            // Elite Aspects
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GenericEnablePatchHook;
        }

        static void Cleanup()
        {
            IL.EntityStates.Scrapper.ScrappingToIdle.OnEnter -= GenericEnablePatchHook;
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GenericEnablePatchHook;
        }
    }
}
#endif