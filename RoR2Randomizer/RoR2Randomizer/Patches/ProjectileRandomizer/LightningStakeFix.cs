using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    // ArchWispCannon (2)

    [PatchClass]
    static class LightningStakeFix
    {
        static int _lightningStakeProjectileIndex = -1;

        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            const string LOG_PREFIX = $"{nameof(LightningStakeFix)}.{nameof(Init)} ";

            const string LIGHTNING_STAKE_PROJECTILE_NAME = "LightningStake";
            _lightningStakeProjectileIndex = ProjectileCatalog.FindProjectileIndex(LIGHTNING_STAKE_PROJECTILE_NAME);
            if (_lightningStakeProjectileIndex == -1)
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
                    if (hasBuff && _lightningStakeProjectileIndex != -1 && ProjectileRandomizerController.IsActive)
                    {
                        if (ProjectileRandomizerController.TryGetOverrideProjectileIndex(_lightningStakeProjectileIndex, out int lightningStakeReplacementIndex))
                        {
                            if (damageInfo.inflictor && damageInfo.inflictor.TryGetComponent<ProjectileController>(out ProjectileController projectileController))
                            {
                                if (projectileController.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker parentChainTracker))
                                {
                                    if (parentChainTracker.IsChildOf(lightningStakeReplacementIndex))
                                    {
#if DEBUG
                                        Log.Debug($"Prevented infinite projectile loop ({projectileController.name} is child of {ProjectileCatalog.GetProjectilePrefab(lightningStakeReplacementIndex)?.name})");
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
        }
    }
}
