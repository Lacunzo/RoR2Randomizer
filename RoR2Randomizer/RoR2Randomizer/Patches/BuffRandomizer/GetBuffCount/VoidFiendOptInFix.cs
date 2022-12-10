using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    static partial class GetBuffIndex_BuffIndex_ReplacePatch
    {
        [PatchClass]
        static class VoidFiendOptInFix
        {
            static void Apply()
            {
                IL.RoR2.VoidSurvivorController.FixedUpdate += VoidSurvivorController_FixedUpdate;
            }

            static void Cleanup()
            {
                IL.RoR2.VoidSurvivorController.FixedUpdate -= VoidSurvivorController_FixedUpdate;
            }

            static void VoidSurvivorController_FixedUpdate(ILContext il)
            {
                const string LOG_PREFIX = $"{nameof(GetBuffIndex_BuffIndex_ReplacePatch)}+{nameof(VoidFiendOptInFix)}.{nameof(VoidSurvivorController_FixedUpdate)} ";

                ILCursor c = new ILCursor(il);

                ILCursor[] foundCursors;

                int patchCount = 0;
                while (c.TryFindNext(out foundCursors,
                                     x => x.MatchLdfld<VoidSurvivorController>(nameof(VoidSurvivorController.corruptedBuffDef)),
                                     x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<CharacterBody>(_ => _.HasBuff(default(BuffDef))))))
                {
                    foundCursors[1].Emit(OpCodes.Call, enablePatch_MI);

                    ILCursor last = foundCursors[foundCursors.Length - 1];
                    last.Index++;
                    last.Emit(OpCodes.Call, disablePatch_MI);

                    c.Index = last.Index;

                    patchCount++;
                }

                if (patchCount == 0)
                {
                    Log.Warning(LOG_PREFIX + "no patch locations found");
                }
#if DEBUG
                else
                {
                    Log.Debug(LOG_PREFIX + $"{patchCount} patch locations found");
                }
#endif
            }
        }
    }
}
