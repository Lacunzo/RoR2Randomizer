using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.Fixes
{
    [PatchClass]
    // Fixes possible infinite while loops when exiting to main menu, certain buffs get removed in a while loop, and if the client does not have authority to remove them, the game loops indefinitely
    static class InfiniteLoopOnExitFix
    {
        static void Apply()
        {
            IL.RoR2.PrimarySkillShurikenBehavior.OnDisable += PrimarySkillShurikenBehavior_OnDisable;
            IL.RoR2.Items.ImmuneToDebuffBehavior.OnDisable += ImmuneToDebuffBehavior_OnDisable;
        }

        static void Cleanup()
        {
            IL.RoR2.PrimarySkillShurikenBehavior.OnDisable -= PrimarySkillShurikenBehavior_OnDisable;
            IL.RoR2.Items.ImmuneToDebuffBehavior.OnDisable -= ImmuneToDebuffBehavior_OnDisable;
        }

        const int MAX_COUNTER = 150;

        static readonly FieldInfo _counter_FI = AccessTools.DeclaredField(typeof(InfiniteLoopOnExitFix), nameof(_counter));
        static int _counter = 0;

        static void emitResetCounter(ILCursor cursor)
        {
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Stsfld, _counter_FI);
        }

        static void PrimarySkillShurikenBehavior_OnDisable(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            emitResetCounter(c);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<CharacterBody>(_ => _.HasBuff(default(BuffDef))))))
            {
                c.EmitDelegate((bool hasBuff) =>
                {
                    return hasBuff && ++_counter < MAX_COUNTER && NetworkServer.active;
                });
            }
        }

        static void ImmuneToDebuffBehavior_OnDisable(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            emitResetCounter(c);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<CharacterBody>(_ => _.GetBuffCount(default(BuffDef))))))
            {
                // If server is not active, buffs cannot be removed, so just pretend there are none (otherwise the game will enter an infinite loop)
                c.EmitDelegate((int buffCount) => ++_counter < MAX_COUNTER && NetworkServer.active ? buffCount : 0);
            }
        }
    }
}
