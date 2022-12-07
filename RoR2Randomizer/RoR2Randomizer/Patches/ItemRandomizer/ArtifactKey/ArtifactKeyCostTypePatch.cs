#if !DISABLE_ITEM_RANDOMIZER
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Patches.Fixes;
using RoR2Randomizer.RandomizerControllers.Item;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.ItemRandomizer.ArtifactKey
{
    [PatchClass]
    static class ArtifactKeyCostTypePatch
    {
        [SystemInitializer(typeof(CostTypeCatalog))]
        static void Init()
        {
            CostTypeDef artifactKeyCostType = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.ArtifactShellKillerItem);
            if (artifactKeyCostType != null)
            {
                static void applyHookIfPatchClassActive(IDetour hook)
                {
                    if (_patchActive)
                    {
                        hook.Apply();
                    }
                }

                CostTypeDef.IsAffordableDelegate isAffordable = artifactKeyCostType.isAffordable;
                if (isAffordable != null)
                {
                    _artifactKeyIsAffordableHook = new ILHook(isAffordable.Method, ArtifactKeyCostType_IsAffordable, new ILHookConfig { ManualApply = true });
                    applyHookIfPatchClassActive(_artifactKeyIsAffordableHook);
                }

                CostTypeDef.PayCostDelegate payCost = artifactKeyCostType.payCost;
                if (payCost != null)
                {
                    _artifactKeyPayCostHook = new Hook(payCost.Method, ArtifactKeyCostType_PayCost, new HookConfig { ManualApply = true });
                    applyHookIfPatchClassActive(_artifactKeyPayCostHook);
                }
            }
        }

        static bool _patchActive = false;

        static ILHook _artifactKeyIsAffordableHook;
        static Hook _artifactKeyPayCostHook;

        static void Apply()
        {
            _artifactKeyIsAffordableHook?.Apply();
            _artifactKeyPayCostHook?.Apply();
            _patchActive = true;
        }

        static void Cleanup()
        {
            _artifactKeyIsAffordableHook?.Undo();
            _artifactKeyPayCostHook?.Undo();
            _patchActive = false;
        }

        static PickupDef _artifactKeyReplacementPickup;

        static void ArtifactKeyCostType_IsAffordable(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.EmitDelegate(static () =>
            {
                if (ItemRandomizerController.IsEnabled)
                {
                    if (ItemRandomizerController.TryGetReplacementPickupIndex(PickupCatalog.FindPickupIndex(RoR2Content.Items.ArtifactKey.itemIndex), out PickupIndex artifactKeyReplacement))
                    {
                        _artifactKeyReplacementPickup = artifactKeyReplacement.pickupDef;
                        if (_artifactKeyReplacementPickup.itemIndex == ItemIndex.None)
                        {
                            return true;
                        }
                    }
                }

                _artifactKeyReplacementPickup = null;
                return false;
            });

            ILLabel origLbl = il.DefineLabel();

            c.Emit(OpCodes.Brfalse, origLbl);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate(static (CostTypeDef.IsAffordableContext context) =>
            {
                if (!context.activator)
                    return false;

                if (!context.activator.TryGetComponent(out CharacterBody body))
                    return false;

                CharacterMaster master = body.master;
                if (!master)
                    return false;

                return master.GetPickupCount(_artifactKeyReplacementPickup) >= context.cost;
            });
            c.Emit(OpCodes.Ret);

            c.Index++;
            origLbl.Target = c.Next;

            while (c.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.ArtifactKey))))
            {
                c.Index++;
                c.EmitDelegate(static (ItemDef artifactKey) =>
                {
                    if (_artifactKeyReplacementPickup != null)
                    {
                        return ItemCatalog.GetItemDef(_artifactKeyReplacementPickup.itemIndex);
                    }

                    return artifactKey;
                });
            }
        }

        static void ArtifactKeyCostType_PayCost(Action<object, CostTypeDef, CostTypeDef.PayCostContext> orig, object self, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            orig(self, costTypeDef, context);
        }
    }
}
#endif