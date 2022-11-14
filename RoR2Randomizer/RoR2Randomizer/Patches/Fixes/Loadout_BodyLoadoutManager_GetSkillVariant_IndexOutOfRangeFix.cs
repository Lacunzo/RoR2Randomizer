using HarmonyLib;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RoR2Randomizer.Patches.Fixes
{
    [PatchClass]
    static class Loadout_BodyLoadoutManager_GetSkillVariant_IndexOutOfRangeFix
    {
        static void Apply()
        {
            IL.RoR2.Loadout.BodyLoadoutManager.GetSkillVariant += BodyLoadoutManager_GetSkillVariant;
        }

        static void Cleanup()
        {
            IL.RoR2.Loadout.BodyLoadoutManager.GetSkillVariant -= BodyLoadoutManager_GetSkillVariant;
        }

        static void BodyLoadoutManager_GetSkillVariant(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchLdelemU4()))
            {
                // Replace Ldelem with GetSafe to prevent IndexOutOfRange errors

                c.Remove();
                c.Emit(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ArrayUtils.GetSafe<uint>(default, default)));
            }
        }
    }
}
