using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.MultiEntityStatePatches
{
    public static class SetSubStatesPatch
    {
        public static void Apply()
        {
            IL.RoR2.CharacterDeathBehavior.OnDeath += CharacterDeathBehavior_OnDeath;
        }

        public static void Cleanup()
        {
            IL.RoR2.CharacterDeathBehavior.OnDeath -= CharacterDeathBehavior_OnDeath;
        }

        static void CharacterDeathBehavior_OnDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCall(SymbolExtensions.GetMethodInfo(() => EntityStateCatalog.InstantiateState(default(SerializableEntityStateType))))))
            {
                c.Index++; // Move after InstantiateState call

                c.Emit(OpCodes.Dup); // Dup EntityState

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((EntityState state, CharacterDeathBehavior instance) =>
                {
                    if (state is MultiEntityState multiState && instance.TryGetComponent<MultiEntityStateSubStatesData>(out MultiEntityStateSubStatesData subStateData))
                    {
                        multiState.SetStates(subStateData.StateTypes);
                    }
                });
            }

            Log.Debug(il.ToString());
        }
    }
}
