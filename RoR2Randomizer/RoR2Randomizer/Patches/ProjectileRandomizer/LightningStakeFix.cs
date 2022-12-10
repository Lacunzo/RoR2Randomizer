using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class LightningStakeFix
    {
        static ProjectileTypeIdentifier _lightningStakeProjectileIdentifier;

        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            const string LOG_PREFIX = $"{nameof(LightningStakeFix)}.{nameof(Init)} ";

            const string LIGHTNING_STAKE_PROJECTILE_NAME = "LightningStake";
            _lightningStakeProjectileIdentifier = new ProjectileTypeIdentifier(ProjectileType.OrdinaryProjectile, ProjectileCatalog.FindProjectileIndex(LIGHTNING_STAKE_PROJECTILE_NAME));
            if (!_lightningStakeProjectileIdentifier.IsValid)
            {
                Log.Warning(LOG_PREFIX + $"Unable to find projectile index for {LIGHTNING_STAKE_PROJECTILE_NAME}");
            }
        }

        static void Apply()
        {
            IL.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;
        }

        static void Cleanup()
        {
            IL.RoR2.GlobalEventManager.OnHitAll -= GlobalEventManager_OnHitAll;
        }

        static void GlobalEventManager_OnHitAll(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(LightningStakeFix)}.{nameof(GlobalEventManager_OnHitAll)} ";

            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.AffixBlue)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<CharacterBody>(_ => _.HasBuff(default(BuffDef))))))
            {
                ILCursor hasBuffHook = foundCursors[1];
                hasBuffHook.Index++;

                hasBuffHook.Emit(OpCodes.Ldarg_1);
                hasBuffHook.EmitDelegate(static (bool hasBuff, DamageInfo damageInfo) =>
                {
                    if (hasBuff && _lightningStakeProjectileIdentifier.IsValid && ProjectileRandomizerController.IsActive)
                    {
                        if (ProjectileRandomizerController.TryGetOverrideProjectileIdentifier(_lightningStakeProjectileIdentifier, out ProjectileTypeIdentifier lightningStakeReplacement))
                        {
                            if (damageInfo.inflictor && damageInfo.inflictor.TryGetComponent<ProjectileController>(out ProjectileController projectileController))
                            {
                                if (projectileController.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker parentChainTracker))
                                {
                                    if (parentChainTracker.IsChildOf(lightningStakeReplacement) || 
                                        (ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile != null &&
                                         ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile.IsChildOf(lightningStakeReplacement)))
                                    {
#if DEBUG
                                        Log.Debug(LOG_PREFIX + $"Prevented infinite projectile loop ({projectileController.name} is child of {lightningStakeReplacement})");
#endif

                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    return hasBuff;
                });
            }
            else
            {
                Log.Warning(LOG_PREFIX + "failed to find patch location");
            }
        }
    }
}
