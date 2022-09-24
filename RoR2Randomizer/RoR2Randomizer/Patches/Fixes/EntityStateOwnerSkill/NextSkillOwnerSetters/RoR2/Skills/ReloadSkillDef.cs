#if !DISABLE_SKILL_RANDOMIZER
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Utility;
using RoR2Randomizer.Utility.Patching;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    public static class ReloadSkillDef
    {
        public static void Apply()
        {
            IL.RoR2.Skills.ReloadSkillDef.OnFixedUpdate += ReloadSkillDef_OnFixedUpdate;
        }

        public static void Cleanup()
        {
            IL.RoR2.Skills.ReloadSkillDef.OnFixedUpdate -= ReloadSkillDef_OnFixedUpdate;
        }

        static void ReloadSkillDef_OnFixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchCall(StaticReflectionCache.EntityStateCatalog_InstantiateState_SerializableEntityStateType_MI)))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((GenericSkill skillSlot) =>
                {
                    EntityStateOwnerTracker.SkillOwnerForNextCall = skillSlot;
                });

                c.Index++; // Move after the call so it does not match the same one again
            }
        }
    }
}
#endif