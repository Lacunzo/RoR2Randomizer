using HG;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
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
                identifiers = identifiers.Concat(BulletAttackCatalog.GetAllBulletAttacks().Select(static b => new ProjectileTypeIdentifier(ProjectileType.Bullet, b.Index)));
            }

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

        static void BulletAttackCatalog_BulletAttackAppended(BulletAttackIdentifier identifier)
        {
            if (Run.instance && NetworkServer.active)
            {
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
        }

        protected override void Awake()
        {
            base.Awake();

            SyncProjectileReplacements.OnReceive += onProjectileReplacementsReceivedFromServer;
            BulletAttackCatalog.BulletAttackAppended += BulletAttackCatalog_BulletAttackAppended;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SyncProjectileReplacements.OnReceive -= onProjectileReplacementsReceivedFromServer;
            BulletAttackCatalog.BulletAttackAppended -= BulletAttackCatalog_BulletAttackAppended;

            _projectileIndicesReplacements.Dispose();
            _hasReceivedProjectileReplacementsFromServer.Dispose();
        }

        public static bool TryReplaceProjectileInstantiateFire(ref GameObject projectilePrefab, out GameObject originalPrefab, Vector3 origin, Quaternion rotation, GameObject owner, float damage, float force, bool isCrit, DamageType? damageType)
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
                        replacement.Fire(origin, rotation, owner, damage, force, isCrit, damageType);
                        return false;
                    default:
                        Log.Warning(LOG_PREFIX + $"unhandled {nameof(ProjectileType)} {replacement.Type}");
                        break;
                }
            }

            return true;
        }

        public static bool TryReplaceFire(FireProjectileInfo info)
        {
            return TryReplaceFire(ProjectileType.OrdinaryProjectile, ProjectileCatalog.GetProjectileIndex(info.projectilePrefab), info.position, info.rotation, info.owner, info.damage, info.force, info.crit, info.damageTypeOverride);
        }

        public static bool TryReplaceFire(ProjectileType type, int index, Vector3 origin, Quaternion rotation, GameObject owner, float damage, float force, bool isCrit, DamageType? damageType)
        {
            if (!IsActive || _replacingTempDisabled)
                return false;

            if (type == ProjectileType.Invalid)
                return false;

            if (type == ProjectileType.Bullet && !ShouldRandomizeBulletAttacks)
                return false;

            if (TryGetOverrideProjectileIdentifier(new ProjectileTypeIdentifier(type, index), out ProjectileTypeIdentifier replacement) && replacement.IsValid)
            {
                _replacingTempDisabled = true;
                replacement.Fire(origin, rotation, owner, damage, force, isCrit, damageType);

                if (type == ProjectileType.OrdinaryProjectile && replacement.Type != ProjectileType.OrdinaryProjectile)
                {
                    GameObject originalProjectilePrefab = ProjectileCatalog.GetProjectilePrefab(index);

                    if (originalProjectilePrefab.GetComponent<ProjectileGrappleController>())
                    {
                        const string STATE_MACHINE_NAME = "Hook";

                        EntityStateMachine hookStateMachine = EntityStateMachine.FindByCustomName(owner, STATE_MACHINE_NAME);
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

                _replacingTempDisabled = false;
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
