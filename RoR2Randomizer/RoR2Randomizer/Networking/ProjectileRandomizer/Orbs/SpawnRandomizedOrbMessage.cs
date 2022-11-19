using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Patches.OrbEffectOverrideTarget;
using RoR2Randomizer.Patches.ProjectileRandomizer.Orbs;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling;
using RoR2Randomizer.RandomizerControllers.Projectile.Orbs.LightningOrbHandling;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer.Orbs
{
    public sealed class SpawnRandomizedOrbMessage : NetworkMessageBase
    {
        const float MAX_TARGET_DISTANCE = 200f;

        static readonly BullseyeSearch _orbTargetSearch = new BullseyeSearch
        {
            minAngleFilter = 0f,
            maxAngleFilter = 7.5f,
            filterByLoS = true,
            minDistanceFilter = 0f,
            maxDistanceFilter = MAX_TARGET_DISTANCE,
            sortMode = BullseyeSearch.SortMode.Distance
        };

        ProjectileTypeIdentifier _orbIdentifier;
        Vector3 _origin;
        float _damage;
        float _force;
        bool _isCrit;
        GenericFireProjectileArgs _genericArgs;

        Vector3? _overrideTargetPosition;

        public SpawnRandomizedOrbMessage()
        {
        }

        public SpawnRandomizedOrbMessage(ProjectileTypeIdentifier orbIdentifier, Vector3 origin, Quaternion rotation, float damage, float force, bool isCrit, GenericFireProjectileArgs genericArgs)
        {
            _orbIdentifier = orbIdentifier;
            _origin = origin;
            _damage = damage;
            _force = force;
            _isCrit = isCrit;
            _genericArgs = genericArgs;

            if (!_genericArgs.Target)
            {
                Vector3 direction = rotation * Vector3.forward;

                _orbTargetSearch.searchOrigin = _origin;
                _orbTargetSearch.searchDirection = direction;

                CharacterBody ownerBody = _genericArgs.OwnerBody;
                if (ownerBody)
                {
                    _orbTargetSearch.viewer = ownerBody;
                    _orbTargetSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(_genericArgs.OwnerTeam);
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
                    _genericArgs.Target = hurtBox;
                }
                else
                {
                    Ray ray = new Ray(_origin, direction);

                    if (Physics.Raycast(ray, out RaycastHit hit, MAX_TARGET_DISTANCE, LayerIndex.world.mask))
                    {
                        _overrideTargetPosition = hit.point;
                    }
                    else
                    {
                        _overrideTargetPosition = ray.GetPoint(MAX_TARGET_DISTANCE);
                    }
                }
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            _orbIdentifier.Serialize(writer);
            writer.Write(_origin);
            writer.Write(_damage);
            writer.Write(_force);
            writer.Write(_isCrit);
            writer.Write(_genericArgs);
            writer.WriteNullableVector3(_overrideTargetPosition);
            }

        public override void Deserialize(NetworkReader reader)
        {
            _orbIdentifier = new ProjectileTypeIdentifier(reader);
            _origin = reader.ReadVector3();
            _damage = reader.ReadSingle();
            _force = reader.ReadSingle();
            _isCrit = reader.ReadBoolean();
            _genericArgs = reader.Read<GenericFireProjectileArgs>();
            _overrideTargetPosition = reader.ReadNullableVector3();
            }

        public override void OnReceived()
        {
            const string LOG_PREFIX = $"{nameof(SpawnRandomizedOrbMessage)}.{nameof(OnReceived)} ";
            if (!NetworkServer.active)
            {
                Log.Warning(LOG_PREFIX + "called on client");
                return;
            }

            spawnOrb();
        }

        public void SpawnOrSendMessage()
        {
            if (NetworkServer.active || !_genericArgs.Target)
            {
                spawnOrb();
            }
            else if (NetworkClient.active)
            {
                SendTo(NetworkDestination.Server);
            }
        }

        void spawnOrb()
        {
            const string LOG_PREFIX = $"{nameof(SpawnRandomizedOrbMessage)}.{nameof(spawnOrb)} ";

            Orb orb;
            switch (_orbIdentifier.Type)
            {
                case ProjectileType.DamageOrb:
                    DamageOrbIdentifier damageOrbIdentifier = DamageOrbCatalog.GetIdentifier(_orbIdentifier.Index);
                    if (!damageOrbIdentifier.IsValid)
                    {
                        Log.Warning(LOG_PREFIX + $"invalid damage orb at index {_orbIdentifier.Index}");
                        return;
                    }

                    GenericDamageOrb damageOrb = damageOrbIdentifier.CreateInstance();

                    damageOrb.damageValue = _damage;
                    damageOrb.attacker = _genericArgs.Owner;
                    damageOrb.isCrit = _isCrit;
                    damageOrb.teamIndex = _genericArgs.OwnerTeam;

                    //if (damageType.HasValue)
                    //    damageOrb.damageType = damageType.Value;

                    orb = damageOrb;
                    break;
                case ProjectileType.LightningOrb:
                    LightningOrbIdentifier lightningOrbIdentifier = LightningOrbCatalog.GetIdentifier(_orbIdentifier.Index);
                    if (!lightningOrbIdentifier.IsValid)
                    {
                        Log.Warning(LOG_PREFIX + $"invalid lightning orb at index {_orbIdentifier.Index}");
                        return;
                    }

                    LightningOrb lightningOrb = lightningOrbIdentifier.CreateInstance();

                    lightningOrb.damageValue = _damage;
                    lightningOrb.attacker = _genericArgs.Owner;
                    lightningOrb.inflictor = _genericArgs.Weapon;
                    lightningOrb.teamIndex = _genericArgs.OwnerTeam;
                    lightningOrb.isCrit = _isCrit;

                    orb = lightningOrb;
                    break;
                default:
                    Log.Warning(LOG_PREFIX + $"unhandled orb type {_orbIdentifier.Type}");
                    return;
            }

            orb.origin = _origin;

            if (orb is SquidOrb squidOrb)
            {
                squidOrb.forceScalar = _force;
            }

            if (_genericArgs.Target)
            {
                orb.target = _genericArgs.Target;
            }
            else
            {
                if (!_overrideTargetPosition.HasValue)
                {
                    Log.Warning(LOG_PREFIX + $"both {nameof(_genericArgs)}.{nameof(_genericArgs.Target)} and {nameof(_overrideTargetPosition)} null");
                }
                else
                {
                    Vector3 position = _overrideTargetPosition.Value;
                    OrbHurtBoxReferenceObjectOverridePatch.overrideOrbTargetPosition[orb] = position;

                    if (orb is LightningStrikeOrb lightningStrikeOrb)
                    {
                        lightningStrikeOrb.lastKnownTargetPosition = position;
                    }
                    else if (orb is SimpleLightningStrikeOrb simpleLightningStrikeOrb)
                    {
                        simpleLightningStrikeOrb.lastKnownTargetPosition = position;
                    }
                }
            }

            OrbManager_AddOrbHook.PatchDisabledCount++;
            OrbManager.instance.AddOrb(orb);
            OrbManager_AddOrbHook.PatchDisabledCount--;
        }
    }
}
