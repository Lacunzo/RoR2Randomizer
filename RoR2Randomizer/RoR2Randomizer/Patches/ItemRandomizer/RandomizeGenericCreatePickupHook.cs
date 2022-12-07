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
        static readonly ILHook PlayerCharacterMasterController_onCharacterDeathGlobal_Hook;

        static RandomizeGenericCreatePickupHook()
        {
            const string LOG_PREFIX = $"{nameof(RandomizeGenericCreatePickupHook)}..cctor ";

            const string TYPE_NAME = "<>c";
            Type type = typeof(PlayerCharacterMasterController).GetNestedType(TYPE_NAME, BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (type != null)
            {
                MethodInfo targetMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).SingleOrDefault(static m =>
                {
                    if (m.ReturnType != typeof(void))
                        return false;

                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameters.Length != 1)
                        return false;

                    if (parameters[0].ParameterType != typeof(DamageReport))
                        return false;

                    return true;
                });

                if (targetMethod != null)
                {
                    PlayerCharacterMasterController_onCharacterDeathGlobal_Hook = new ILHook(targetMethod, GenericEnablePatchHook, new ILHookConfig { ManualApply = true });
                }
                else
                {
                    Log.Warning(LOG_PREFIX + $"unable to find method");
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"unable to find nested type {TYPE_NAME}");
            }
        }

        static void Apply()
        {
            IL.EntityStates.VoidCamp.Deactivate.OnEnter += GenericEnablePatchHook;
            IL.RoR2.InfiniteTowerWaveController.DropRewards += GenericEnablePatchHook;
            IL.EntityStates.Scrapper.ScrappingToIdle.OnEnter += GenericEnablePatchHook;
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GenericEnablePatchHook;

            PlayerCharacterMasterController_onCharacterDeathGlobal_Hook?.Apply();
        }

        static void Cleanup()
        {
            IL.EntityStates.VoidCamp.Deactivate.OnEnter -= GenericEnablePatchHook;
            IL.RoR2.InfiniteTowerWaveController.DropRewards -= GenericEnablePatchHook;
            IL.EntityStates.Scrapper.ScrappingToIdle.OnEnter -= GenericEnablePatchHook;
            IL.RoR2.GlobalEventManager.OnCharacterDeath -= GenericEnablePatchHook;

            PlayerCharacterMasterController_onCharacterDeathGlobal_Hook?.Undo();
        }
    }
}
#endif