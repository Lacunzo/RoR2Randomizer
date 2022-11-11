using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2Randomizer.Patches.OrbEffectOverrideTarget;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    public readonly struct ProjectileTypeIdentifier : IEquatable<ProjectileTypeIdentifier>
    {
        public static readonly ProjectileTypeIdentifier Invalid = new ProjectileTypeIdentifier(ProjectileType.Invalid, -1);

        static readonly BullseyeSearch _orbTargetSearch = new BullseyeSearch
        {
            minAngleFilter = 0f,
            maxAngleFilter = 7.5f,
            filterByLoS = true,
            minDistanceFilter = 0f,
            maxDistanceFilter = float.PositiveInfinity,
            sortMode = BullseyeSearch.SortMode.Distance
        };

        public readonly ProjectileType Type;
        public readonly int Index;

        public readonly bool IsValid => Type != ProjectileType.Invalid && Index != -1;

        public ProjectileTypeIdentifier(ProjectileType type, int index)
        {
            Type = type;
            Index = index;
        }

        public ProjectileTypeIdentifier(NetworkReader reader)
        {
            Type = (ProjectileType)reader.ReadPackedIndex32();
            Index = reader.ReadPackedIndex32();
        }

        public static ProjectileTypeIdentifier FromProjectilePrefab(GameObject prefab)
        {
            return new ProjectileTypeIdentifier(ProjectileType.OrdinaryProjectile, ProjectileCatalog.GetProjectileIndex(prefab));
        }

        public readonly void Serialize(NetworkWriter writer)
        {
            writer.WritePackedIndex32((int)Type);
            writer.WritePackedIndex32(Index);
        }

        public readonly void Fire(Vector3 origin, Quaternion rotation, float damage, float force, bool isCrit, GenericFireProjectileArgs genericArgs)
        {
            const string LOG_PREFIX = $"{nameof(ProjectileTypeIdentifier)}.{nameof(Fire)} ";

            genericArgs.ModifyArgs(ref origin);

#if DEBUG
            Log.Debug($"Firing {Type} in direction {rotation.eulerAngles}");
#endif

            //if (damageType.HasValue && damageType.Value == DamageType.Generic)
            //    damageType = null;

            Vector3 direction = (rotation * Vector3.forward).normalized;

            CharacterBody ownerBody = genericArgs.OwnerBody;

            switch (Type)
            {
                case ProjectileType.OrdinaryProjectile:
                    ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                    {
                        projectilePrefab = ProjectileCatalog.GetProjectilePrefab(Index),
                        crit = isCrit,
                        damage = damage,
                        force = force,
                        owner = genericArgs.Owner,
                        position = origin,
                        rotation = rotation,
                        target = genericArgs.TargetGO,
                        //damageTypeOverride = damageType
                    });
                    break;
                case ProjectileType.Bullet:
                    BulletAttackIdentifier bulletIdentifier = BulletAttackCatalog.GetBulletAttack(Index);
                    if (!bulletIdentifier.IsValid)
                    {
                        Log.Warning(LOG_PREFIX + $"invalid bullet attack at index {Index}");
                        break;
                    }

                    BulletAttack bulletAttack = bulletIdentifier.CreateInstance();
                    bulletAttack.aimVector = direction;
                    bulletAttack.bulletCount = 1;
                    bulletAttack.damage = damage;
                    bulletAttack.force = force;
                    bulletAttack.hitEffectPrefab = EffectCatalog.GetEffectDef(bulletIdentifier.HitEffectIndex)?.prefab;
                    bulletAttack.isCrit = isCrit;
                    bulletAttack.origin = origin;
                    bulletAttack.owner = genericArgs.Owner;
                    bulletAttack.weapon = genericArgs.Weapon;
                    bulletAttack.muzzleName = genericArgs.MuzzleName;
                    bulletAttack.tracerEffectPrefab = EffectCatalog.GetEffectDef(bulletIdentifier.TracerEffectIndex)?.prefab;
                    
                    //if (damageType.HasValue)
                    //    bulletAttack.damageType |= damageType.Value;
                    
                    bulletAttack.Fire();
                    break;
                case ProjectileType.DamageOrb:
                    DamageOrbIdentifier orbIdentifier = DamageOrbCatalog.GetIdentifier(Index);
                    if (!orbIdentifier.IsValid)
                    {
                        Log.Warning(LOG_PREFIX + $"invalid damage orb at index {Index}");
                        break;
                    }

                    GenericDamageOrb damageOrb = orbIdentifier.CreateInstance();

                    damageOrb.origin = origin;
                    damageOrb.damageValue = damage;
                    damageOrb.attacker = genericArgs.Owner;
                    damageOrb.isCrit = isCrit;

                    //if (damageType.HasValue)
                    //    damageOrb.damageType = damageType.Value;

                    if (damageOrb is SquidOrb squidOrb)
                    {
                        squidOrb.forceScalar = force;
                    }

                    if (genericArgs.Target)
                    {
                        damageOrb.target = genericArgs.Target;
                    }
                    else
                    {
                        _orbTargetSearch.searchOrigin = origin;
                        _orbTargetSearch.searchDirection = direction;

                        if (ownerBody)
                        {
                            _orbTargetSearch.viewer = ownerBody;
                            _orbTargetSearch.teamMaskFilter = TeamMask.allButNeutral;
                            _orbTargetSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(ownerBody.gameObject));
                        }
                        else
                        {
                            _orbTargetSearch.viewer = null;
                            _orbTargetSearch.teamMaskFilter = TeamMask.all;
                        }

                        _orbTargetSearch.RefreshCandidates();

                        HurtBox hurtBox = _orbTargetSearch.GetResults().FirstOrDefault();
                        if (hurtBox)
                        {
                            damageOrb.target = hurtBox;
                        }
                        else
                        {
                            const float MAX_DISTANCE = 100f;

                            Ray ray = new Ray(origin, direction);

                            Vector3 targetPosition;
                            if (Physics.Raycast(ray, out RaycastHit hit, MAX_DISTANCE, LayerIndex.world.mask))
                            {
                                targetPosition = hit.point;
                            }
                            else
                            {
                                targetPosition = ray.GetPoint(MAX_DISTANCE);
                            }

                            DamageOrbHurtBoxReferenceObjectOverridePatch.overrideOrbTargetPosition[damageOrb] = targetPosition;

                            if (damageOrb is LightningStrikeOrb lightningStrikeOrb)
                            {
                                lightningStrikeOrb.lastKnownTargetPosition = targetPosition;
                            }
                            else if (damageOrb is SimpleLightningStrikeOrb simpleLightningStrikeOrb)
                            {
                                simpleLightningStrikeOrb.lastKnownTargetPosition = targetPosition;
                            }
                        }
                    }

                    OrbManager.instance.AddOrb(damageOrb);
                    break;
                default:
                    Log.Warning(LOG_PREFIX + $"unhandled type {Type}");
                    break;
            }
        }

        public readonly bool Equals(ProjectileTypeIdentifier other)
        {
            return Type == other.Type && Index == other.Index;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ProjectileTypeIdentifier other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            int hashCode = 686506176;
            hashCode = (hashCode * -1521134295) + Type.GetHashCode();
            hashCode = (hashCode * -1521134295) + Index.GetHashCode();
            return hashCode;
        }

        public override readonly string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{Type}: {Index}");

            if (Type == ProjectileType.OrdinaryProjectile)
            {
                sb.Append($" ({ProjectileCatalog.GetProjectilePrefab(Index)?.name ?? "null"})");
            }

            return sb.ToString();
        }

        public static bool operator ==(ProjectileTypeIdentifier a, ProjectileTypeIdentifier b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProjectileTypeIdentifier a, ProjectileTypeIdentifier b)
        {
            return !a.Equals(b);
        }
    }
}
