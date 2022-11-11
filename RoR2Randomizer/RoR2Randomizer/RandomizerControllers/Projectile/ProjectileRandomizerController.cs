using HG;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using RoR2Randomizer.RandomizerControllers.Projectile.DamageOrbHandling;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    [RandomizerController]
    public class ProjectileRandomizerController : BaseRandomizerController
    {
        static int[] _projectileIndicesToRandomize;

        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            _projectileIndicesToRandomize =
                ProjectileCatalog.projectilePrefabProjectileControllerComponents
                                 .Where(projectile =>
                                 {
                                     if (!projectile)
                                         return false;

                                     if (projectile.TryGetComponent<ProjectileFireChildren>(out ProjectileFireChildren projectileFireChildren)
                                      && (!projectileFireChildren.childProjectilePrefab || projectileFireChildren.childProjectilePrefab == null))
                                     {
#if DEBUG                            
                                         Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to invalid {nameof(ProjectileFireChildren)} setup");
#endif

                                         return false;
                                     }

                                     switch (projectile.name)
                                     {
                                         case "AACannon": // Does nothing
                                         case "AncientWispCannon": // Does nothing
                                         case "BanditBomblets": // Does nothing
                                         case "BanditClusterBombSeed": // Clusterbombs fall through ground and do nothing
                                         case "BanditClusterGrenadeProjectile": // No collision, cannot deal damage
                                         case "BeetleQueenAcid": // Does nothing
                                         case "BellBallSmall": // Does nothing
                                         case "DroneRocket": // Does nothing
                                         case "EngiMineDeployer": // Constant NullRef in FixedUpdate
                                         case "EngiSeekerGrenadeProjectile": // Does nothing
                                         case "EngiWallShield": // Unfinished engi shield
                                         case "GatewayProjectile": // Does nothing
                                         case "MinorConstructOnKillProjectile": // Does nothing
                                         case "NullifierBombProjectile": // Does nothing
                                         case "PaladinBigRocket": // Does nothing
                                         case "RedAffixMissileProjectile": // Does nothing
                                         case "ScoutGrenade": // Does nothing
                                         case "Rocket": // Does nothing
                                         case "Spine": // No collision, cannot deal damage
                                         case "ToolbotDroneHeal": // Does nothing
                                         case "ToolbotDroneStun": // Does nothing
                                         case "TreebotPounderProjectile": // Does nothing

                                         // Excluded because I think it's more fun that way
                                         case "MageIcewallWalkerProjectile":
                                         case "MageFirewallWalkerProjectile":

                                         // Excluded because it seems like a huge pain getting it to work, might look into it in the future.
                                         case "LunarSunProjectile":
#if DEBUG                            
                                             Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to being in blacklist");
#endif                               
                                             return false;
                                     }

#if DEBUG
                                     if (projectile.TryGetComponent<ProjectileSpawnMaster>(out ProjectileSpawnMaster projectileSpawnMaster))
                                     {
                                         Log.Debug($"{projectile.name} spawns {projectileSpawnMaster.spawnCard} ({projectileSpawnMaster.spawnCard?.prefab})");
                                     }

                                     if (projectileFireChildren)
                                     {
                                         Log.Debug($"Projectile Randomizer: {projectile.name} is {nameof(ProjectileFireChildren)}");
                                     }
#endif

                                     return true;
                                 })
                                 .Select(p => p.catalogIndex)
                                 .ToArray();
        }

        static readonly RunSpecific<bool> _hasReceivedProjectileReplacementsFromServer = new RunSpecific<bool>();

        static IEnumerable<ProjectileTypeIdentifier> getAllProjectileIdentifiers()
        {
            IEnumerable<ProjectileTypeIdentifier> identifiers = _projectileIndicesToRandomize.Select(static i => new ProjectileTypeIdentifier(ProjectileType.OrdinaryProjectile, i));

            if (ConfigManager.ProjectileRandomizer.RandomizeHitscanAttacks)
            {
                identifiers = identifiers.Concat(BulletAttackCatalog.GetAllBulletAttackProjectileIdentifiers());
            }

            identifiers = identifiers.Concat(DamageOrbCatalog.GetAllDamageOrbProjectileIdentifiers());

            return identifiers;
        }

        static readonly RunSpecific<ReplacementDictionary<ProjectileTypeIdentifier>> _projectileIndicesReplacements = new RunSpecific<ReplacementDictionary<ProjectileTypeIdentifier>>((out ReplacementDictionary<ProjectileTypeIdentifier> result) =>
        {
            if (shouldBeActive)
            {
                result = ReplacementDictionary<ProjectileTypeIdentifier>.CreateFrom(getAllProjectileIdentifiers());
                return true;
            }

            result = default;
            return false;
        });

        static readonly RunSpecific<Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier>> _appendedProjectileReplacements = new RunSpecific<Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier>>();

        static readonly RunSpecific<bool> _shouldRandomizeHitscanServer = new RunSpecific<bool>((out bool result) =>
        {
            if (NetworkServer.active)
            {
                result = ConfigManager.ProjectileRandomizer.RandomizeHitscanAttacks;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        });

        static bool shouldBeActive => NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled;
        public static bool IsActive => (shouldBeActive || (NetworkClient.active && _hasReceivedProjectileReplacementsFromServer)) && _projectileIndicesReplacements.HasValue;

        public static bool ShouldRandomizeBulletAttacks => IsActive && _shouldRandomizeHitscanServer;

        public override bool IsRandomizerEnabled => IsActive;

        protected override bool isNetworked => true;

        static bool _replacingTempDisabled = false;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
#if DEBUG
            Log.Debug($"Sending {nameof(SyncProjectileReplacements)} to clients");
#endif

            if (_projectileIndicesReplacements.HasValue)
            {
                yield return new SyncProjectileReplacements(_projectileIndicesReplacements, false);
            }

            if (_appendedProjectileReplacements.HasValue)
            {
                yield return new SyncProjectileReplacements(new ReplacementDictionary<ProjectileTypeIdentifier>(_appendedProjectileReplacements.Value), true);
            }
        }

        static void onProjectileReplacementsReceivedFromServer(ReplacementDictionary<ProjectileTypeIdentifier> replacements, bool isAppendedReplacements)
        {
            if (isAppendedReplacements)
            {
                _appendedProjectileReplacements.Value = new Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier>(replacements);
            }
            else
            {
                _projectileIndicesReplacements.Value = replacements;
                _hasReceivedProjectileReplacementsFromServer.Value = true;
            }

            if (!_shouldRandomizeHitscanServer)
            {
                _shouldRandomizeHitscanServer.Value |= replacements.Keys.Any(static i => i.Type == ProjectileType.Bullet);
            }
        }

        static void appendProjectileReplacement(ProjectileTypeIdentifier identifier)
        {
            if (!NetworkServer.active)
                return;

            if (!_appendedProjectileReplacements.HasValue)
                _appendedProjectileReplacements.Value = new Dictionary<ProjectileTypeIdentifier, ProjectileTypeIdentifier>();

            if (!_appendedProjectileReplacements.Value.ContainsKey(identifier))
            {
                _appendedProjectileReplacements.Value.Add(identifier, getAllProjectileIdentifiers().GetRandomOrDefault());

                if (!NetworkServer.dontListen)
                {
                    new SyncProjectileReplacements(new ReplacementDictionary<ProjectileTypeIdentifier>(_appendedProjectileReplacements.Value), true).SendTo(NetworkDestination.Clients);
                }
            }
        }

        static void BulletAttackCatalog_BulletAttackAppended(BulletAttackIdentifier identifier)
        {
            if (Run.instance && NetworkServer.active)
            {
                appendProjectileReplacement(identifier);
            }
        }

        static void DamageOrbCatalog_DamageOrbAppendedServer(DamageOrbIdentifier identifier)
        {
            if (Run.instance)
            {
                appendProjectileReplacement(identifier);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            SyncProjectileReplacements.OnReceive += onProjectileReplacementsReceivedFromServer;
            BulletAttackCatalog.BulletAttackAppended += BulletAttackCatalog_BulletAttackAppended;
            DamageOrbCatalog.DamageOrbAppendedServer += DamageOrbCatalog_DamageOrbAppendedServer;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SyncProjectileReplacements.OnReceive -= onProjectileReplacementsReceivedFromServer;
            BulletAttackCatalog.BulletAttackAppended -= BulletAttackCatalog_BulletAttackAppended;
            DamageOrbCatalog.DamageOrbAppendedServer -= DamageOrbCatalog_DamageOrbAppendedServer;

            _projectileIndicesReplacements.Dispose();
            _appendedProjectileReplacements.Dispose();
            _shouldRandomizeHitscanServer.Dispose();
            _hasReceivedProjectileReplacementsFromServer.Dispose();
        }

        public static bool TryReplaceProjectileInstantiateFire(ref GameObject projectilePrefab, out GameObject originalPrefab, Vector3 origin, Quaternion rotation, float damage, float force, bool isCrit, GenericFireProjectileArgs genericArgs)
        {
            const string LOG_PREFIX = $"{nameof(ProjectileRandomizerController)}.{nameof(TryReplaceProjectileInstantiateFire)} ";

            originalPrefab = projectilePrefab;

            if (TryGetOverrideProjectileIdentifier(ProjectileTypeIdentifier.FromProjectilePrefab(projectilePrefab), out ProjectileTypeIdentifier replacement))
            {
                switch (replacement.Type)
                {
                    case ProjectileType.OrdinaryProjectile:
                        projectilePrefab = ProjectileCatalog.GetProjectilePrefab(replacement.Index);
                        return true;
                    case ProjectileType.Bullet:
                    case ProjectileType.DamageOrb:
                        _replacingTempDisabled = true;
                        replacement.Fire(origin, rotation, damage, force, isCrit, genericArgs);
                        _replacingTempDisabled = false;
                        return false;
                    default:
                        Log.Warning(LOG_PREFIX + $"unhandled {nameof(ProjectileType)} {replacement.Type}");
                        break;
                }
            }

            return true;
        }

        public static bool TryReplaceFire(Orb orb)
        {
            const string LOG_PREFIX = $"{nameof(ProjectileRandomizerController)}.{nameof(TryReplaceFire)}({nameof(Orb)}) ";

            if (orb is GenericDamageOrb damageOrb)
            {
                float force;
                if (damageOrb is SquidOrb squidOrb)
                {
                    force = squidOrb.forceScalar;
                }
                else
                {
                    force = 0f;
                }

                Quaternion rotation;
                if (orb.target)
                {
                    rotation = Util.QuaternionSafeLookRotation((orb.target.transform.position - orb.origin).normalized);
                }
                else
                {
                    rotation = Quaternion.identity;
                }

                DamageOrbIdentifier identifier = DamageOrbCatalog.GetIdentifier(damageOrb);
                return identifier.IsValid && TryReplaceFire(identifier, orb.origin, rotation, damageOrb.damageValue, force, damageOrb.isCrit, new GenericFireProjectileArgs
                {
                    Owner = damageOrb.attacker,
                    DamageType = damageOrb.damageType,
                    Target = damageOrb.target
                });
            }
            else
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"unhandled Orb type {orb?.GetType()?.FullName ?? "null"}");
#endif
                return false;
            }
        }

        public static bool TryReplaceFire(BulletAttack bulletAttack, Vector3 fireDirection)
        {
            BulletAttackIdentifier identifier = BulletAttackCatalog.GetBulletAttackIdentifier(bulletAttack);
            return identifier.IsValid && TryReplaceFire(identifier, bulletAttack.origin, Util.QuaternionSafeLookRotation(fireDirection), bulletAttack.damage, bulletAttack.force, bulletAttack.isCrit, new GenericFireProjectileArgs(bulletAttack));
        }

        public static bool TryReplaceFire(FireProjectileInfo info, GameObject weapon)
        {
            HurtBox targetHurtBox = null;
            if (info.target)
            {
                if (info.target.TryGetComponent<HurtBox>(out HurtBox hurtBox))
                {
                    targetHurtBox = hurtBox;
                }
                else if (info.target.TryGetComponent<HurtBoxGroup>(out HurtBoxGroup hurtBoxGroup))
                {
                    targetHurtBox = hurtBoxGroup.hurtBoxes.GetRandomOrDefault();
                }
                else
                {
                    if (!info.target.TryGetComponent<CharacterBody>(out CharacterBody body))
                    {
                        if (info.target.TryGetComponent<CharacterMaster>(out CharacterMaster master))
                        {
                            body = master.GetBody();
                        }
                    }

                    if (body)
                    {
                        targetHurtBox = body.mainHurtBox;
                    }
                }
            }

            return TryReplaceFire(ProjectileTypeIdentifier.FromProjectilePrefab(info.projectilePrefab), info.position, info.rotation, info.damage, info.force, info.crit, new GenericFireProjectileArgs
            {
                Owner = info.owner,
                Weapon = weapon,
                DamageType = info.damageTypeOverride,
                Target = targetHurtBox
            });
        }

        public static bool TryReplaceFire(ProjectileTypeIdentifier identifier, Vector3 origin, Quaternion rotation, float damage, float force, bool isCrit, GenericFireProjectileArgs genericArgs)
        {
            if (!IsActive || _replacingTempDisabled)
                return false;

            if (identifier.Type == ProjectileType.Invalid)
                return false;

            if (identifier.Type == ProjectileType.Bullet && !ShouldRandomizeBulletAttacks)
                return false;

            if (TryGetOverrideProjectileIdentifier(identifier, out ProjectileTypeIdentifier replacement) && replacement.IsValid)
            {
                _replacingTempDisabled = true;
                replacement.Fire(origin, rotation, damage, force, isCrit, genericArgs);
                _replacingTempDisabled = false;

                if (genericArgs.Owner && identifier.Type == ProjectileType.OrdinaryProjectile && replacement.Type != ProjectileType.OrdinaryProjectile)
                {
                    GameObject originalProjectilePrefab = ProjectileCatalog.GetProjectilePrefab(identifier.Index);

                    if (originalProjectilePrefab.GetComponent<ProjectileGrappleController>())
                    {
                        const string STATE_MACHINE_NAME = "Hook";

                        EntityStateMachine hookStateMachine = EntityStateMachine.FindByCustomName(genericArgs.Owner, STATE_MACHINE_NAME);
                        if (hookStateMachine)
                        {
                            if (hookStateMachine.state is EntityStates.Loader.FireHook fireHook)
                            {
                                // Force Hook to retract next update
                                fireHook.hadHookInstance = true;
                            }
                        }
                        else
                        {
                            Log.Warning($"Tried to initialize grapple {replacement.Type}, but owner has no '{STATE_MACHINE_NAME}' state machine");
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public static bool TryGetOriginalProjectilePrefab(GameObject replacementPrefab, out GameObject originalPrefab)
        {
            if (IsActive)
            {
                int projectileIndex = ProjectileCatalog.GetProjectileIndex(replacementPrefab);
                if (TryGetOriginalProjectileIdentifier(new ProjectileTypeIdentifier(ProjectileType.OrdinaryProjectile, projectileIndex), out ProjectileTypeIdentifier original))
                {
                    if (original.Type == ProjectileType.OrdinaryProjectile)
                    {
                        originalPrefab = ProjectileCatalog.GetProjectilePrefab(original.Index);
                        return (bool)originalPrefab;
                    }
                }
            }

            originalPrefab = default;
            return false;
        }

        public static bool TryGetOriginalProjectileIdentifier(ProjectileTypeIdentifier replacement, out ProjectileTypeIdentifier original)
        {
            if (IsActive)
            {
                return _projectileIndicesReplacements.Value.TryGetOriginal(replacement, out original);
            }

            original = default;
            return false;
        }

        public static bool TryGetOverrideProjectileIdentifier(ProjectileTypeIdentifier original, out ProjectileTypeIdentifier replacement)
        {
            if (IsActive)
            {
                if (_projectileIndicesReplacements.Value.TryGetReplacement(original, out replacement) ||
                    (_appendedProjectileReplacements.HasValue && _appendedProjectileReplacements.Value.TryGetValue(original, out replacement)))
                {
#if DEBUG
                    Log.Debug($"Projectile Randomizer: Replaced projectile {original} -> {replacement}");
#endif

                    return true;
                }
            }

            replacement = default;
            return false;
        }
    }
}
