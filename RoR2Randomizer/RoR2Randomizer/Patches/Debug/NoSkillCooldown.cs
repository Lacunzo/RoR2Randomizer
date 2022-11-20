#if DEBUG
using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2;
using System;

namespace RoR2Randomizer.Patches.Debug
{
    [PatchClass]
    static class NoSkillCooldown
    {
        public static void ToggleEnabled()
        {
            _enabled = !_enabled;
            Log.Debug($"No Skill Colldown {(_enabled ? "Enabled" : "Disabled")}");
        }

        static bool _enabled;

        static readonly Hook GenericSkill_get_stock_Hook = new Hook(AccessTools.PropertyGetter(typeof(GenericSkill), nameof(GenericSkill.stock)), (Func<GenericSkill, int> orig, GenericSkill self) =>
        {
            return _enabled ? self.maxStock : orig(self);
        }, new HookConfig() { ManualApply = true });

        static void Apply()
        {
            GenericSkill_get_stock_Hook.Apply();
        }

        static void Cleanup()
        {
            GenericSkill_get_stock_Hook.Undo();
        }
    }
}
#endif