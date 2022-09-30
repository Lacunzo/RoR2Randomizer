using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.CustomContent;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.MultiEntityStatePatches
{
    [PatchClass]
    public static class SetStateOuterPatch
    {
        static void Apply()
        {
            IL.RoR2.EntityStateMachine.Awake += setOuterHook;
            IL.RoR2.EntityStateMachine.SetState += setOuterHook;
            IL.RoR2.NetworkStateMachine.HandleSetEntityState += setOuterHook;
            IL.RoR2.NetworkStateMachine.OnDeserialize += setOuterHook;
        }

        static void Cleanup()
        {
            IL.RoR2.EntityStateMachine.Awake -= setOuterHook;
            IL.RoR2.EntityStateMachine.SetState -= setOuterHook;
            IL.RoR2.NetworkStateMachine.HandleSetEntityState -= setOuterHook;
            IL.RoR2.NetworkStateMachine.OnDeserialize -= setOuterHook;
        }

        static readonly FieldInfo _tempStackField_FI = AccessTools.DeclaredField(typeof(SetStateOuterPatch), nameof(_tempStackField));

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Field is never assigned
        static EntityStateMachine _tempStackField;
#pragma warning restore CS0649 // Field is never assigned
#pragma warning restore IDE0044 // Add readonly modifier

        static void setOuterHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchStfld<EntityState>(nameof(EntityState.outer))))
            {
                // Store EntityStateMachine value in a field to temporarily remove it from the stack
                c.Emit(OpCodes.Stsfld, _tempStackField_FI);

                c.Emit(OpCodes.Dup); // Duplicate the EntityState instance

                c.Emit(OpCodes.Ldsfld, _tempStackField_FI); // Load the EntityStateMachine value back into the stack

                // Go to after the outer field set
                c.Index++;

                c.EmitDelegate((EntityState state) =>
                {
                    if (state is MultiEntityState multiState)
                    {
                        multiState.OnOuterStateMachineAssigned();
                    }
                });
            }
        }
    }
}
