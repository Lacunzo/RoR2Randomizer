#if DEBUG
using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.Debug
{
    [PatchClass]
    static class InfiniteLunarCoin
    {
        public static void ToggleEnabled()
        {
            _enabled = !_enabled;
            Log.Debug($"Infinite Lunar Coins {(_enabled ? "Enabled" : "Disabled")}");
        }

        static bool _enabled;

        //static readonly Hook CostTypeCatalog_Init_b__5_10_Hook = new Hook(typeof(CostTypeCatalog).GetNestedType("<>c", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetMethod("<Init>b__5_10", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly), CostTypeCatalog_Init_MI, new ILHookConfig { ManualApply = true });

        static void Apply()
        {
            On.RoR2.BazaarUpgradeInteraction.CanBeAffordedByInteractor += BazaarUpgradeInteraction_CanBeAffordedByInteractor;

            On.RoR2.NetworkUser.RpcDeductLunarCoins += NetworkUser_RpcDeductLunarCoins;

            //CostTypeCatalog_Init_b__5_10_Hook.Apply();
        }

        static void Cleanup()
        {
            On.RoR2.BazaarUpgradeInteraction.CanBeAffordedByInteractor -= BazaarUpgradeInteraction_CanBeAffordedByInteractor;

            On.RoR2.NetworkUser.RpcDeductLunarCoins -= NetworkUser_RpcDeductLunarCoins;

            //CostTypeCatalog_Init_b__5_10_Hook.Undo();
        }

        static bool BazaarUpgradeInteraction_CanBeAffordedByInteractor(On.RoR2.BazaarUpgradeInteraction.orig_CanBeAffordedByInteractor orig, BazaarUpgradeInteraction self, Interactor activator)
        {
            return orig(self, activator) || _enabled;
        }

        static void NetworkUser_RpcDeductLunarCoins(On.RoR2.NetworkUser.orig_RpcDeductLunarCoins orig, NetworkUser self, uint count)
        {
            if (_enabled)
                return;

            orig(self, count);
        }

        static readonly MethodInfo CostTypeCatalog_Init_MI = SymbolExtensions.GetMethodInfo(() => CostTypeCatalog_Init(default, default, default, default));
        static bool CostTypeCatalog_Init(Func<object, CostTypeDef, CostTypeDef.IsAffordableContext, bool> orig, object self, CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
            return orig(self, costTypeDef, context) || _enabled;
        }
    }
}
#endif