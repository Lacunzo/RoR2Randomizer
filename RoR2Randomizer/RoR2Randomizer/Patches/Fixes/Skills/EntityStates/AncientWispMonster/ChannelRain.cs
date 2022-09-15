using HarmonyLib;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.AncientWispMonster
{
    public static class ChannelRain
    {
        public static void Apply()
        {
            IL.EntityStates.AncientWispMonster.ChannelRain.PlaceRain += Shared.Physics_Raycast_LayerMaskDistanceFix_ILPatch;
        }

        public static void Cleanup()
        {
            IL.EntityStates.AncientWispMonster.ChannelRain.PlaceRain -= Shared.Physics_Raycast_LayerMaskDistanceFix_ILPatch;
        }
    }
}
