using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Artifacts;
using RoR2Randomizer.Extensions;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class EvolutionArtifactGenericPickupFix
    {
        static void Apply()
        {
            IL.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.GrantMonsterTeamItem += MonsterTeamGainsItemsArtifactManager_GrantMonsterTeamItem;
            On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.OnServerCardSpawnedGlobal += MonsterTeamGainsItemsArtifactManager_OnServerCardSpawnedGlobal;
        }

        static void Cleanup()
        {
            IL.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.GrantMonsterTeamItem -= MonsterTeamGainsItemsArtifactManager_GrantMonsterTeamItem;
            On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.OnServerCardSpawnedGlobal -= MonsterTeamGainsItemsArtifactManager_OnServerCardSpawnedGlobal;
        }

        static void MonsterTeamGainsItemsArtifactManager_GrantMonsterTeamItem(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;

            int pickupDefLocalIndex = -1;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => PickupCatalog.GetPickupDef(default))),
                              x => x.MatchStloc(out pickupDefLocalIndex)))
            {
#if DEBUG
                Log.Debug($"{nameof(pickupDefLocalIndex)}={pickupDefLocalIndex}");
#endif

                ILLabel skipGiveItemLabel = null;
                if (c.TryFindNext(out foundCursors,
                                  x => x.MatchLdloc(pickupDefLocalIndex),
                                  x => x.MatchBrfalse(out skipGiveItemLabel)))
                {
                    ILCursor cursor = foundCursors[1];
                    cursor.Index++;

                    cursor.Emit(OpCodes.Ldloc, pickupDefLocalIndex);
                    cursor.EmitDelegate((PickupDef pickup) =>
                    {
                        if (pickup == null ||
                            pickup.itemIndex != ItemIndex.None) // If item: Use default code
                        {
                            return false;
                        }

                        return pickup.TryGrantTo(MonsterTeamGainsItemsArtifactManager.monsterTeamInventory, 1, true);
                    });
                    cursor.Emit(OpCodes.Brtrue, skipGiveItemLabel);
                }
            }
            else
            {
                Log.Error("Unable to find pickupDef local index");
            }
        }

        static void MonsterTeamGainsItemsArtifactManager_OnServerCardSpawnedGlobal(On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.orig_OnServerCardSpawnedGlobal orig, SpawnCard.SpawnResult spawnResult)
        {
            orig(spawnResult);

            if (spawnResult.spawnedInstance && spawnResult.spawnedInstance.TryGetComponent(out CharacterMaster characterMaster))
            {
                if (characterMaster.teamIndex == TeamIndex.Monster)
                {
                    characterMaster.inventory.CopyEquipmentFrom(MonsterTeamGainsItemsArtifactManager.monsterTeamInventory);
                    
                    if (characterMaster.inventory.currentEquipmentIndex != EquipmentIndex.None)
                    {
                        // You wanted AI to activate equipment? Too bad, can't be bothered B)
                        characterMaster.inventory.GiveItemIfMissing(RoR2Content.Items.AutoCastEquipment);
                    }
                }
            }
        }
    }
}
