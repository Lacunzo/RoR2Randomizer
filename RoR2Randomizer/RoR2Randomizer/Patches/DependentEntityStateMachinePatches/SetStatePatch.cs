#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Utility;
using System;

namespace RoR2Randomizer.Patches.DependentEntityStateMachinePatches
{
    [PatchClass]
    static class SetStatePatch
    {
        static readonly Hook EntityStateMachine_set_state_Hook = new Hook(AccessTools.DeclaredPropertySetter(typeof(EntityStateMachine), nameof(EntityStateMachine.state)), (Action<EntityStateMachine, EntityState> orig, EntityStateMachine self, EntityState value) =>
        {
            DependentEntityStateMachine.OnEntityStateAssigned(self, value);
            orig(self, value);
        }, new HookConfig { ManualApply = true });

        static void Apply()
        {
            EntityStateMachine_set_state_Hook.Apply();
        }

        static void Cleanup()
        {
            EntityStateMachine_set_state_Hook.Undo();
        }
    }
}
#endif