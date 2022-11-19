using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Artifacts;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Patches.ProjectileRandomizer.SpiteBomb;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ProjectileRandomizer.SpiteBomb
{
    public sealed class SpawnRandomizedSpiteBombMessage : NetworkMessageBase
    {
        float _damage;
        Vector3 _spawnPosition;
        GenericFireProjectileArgs _genericArgs;

        public SpawnRandomizedSpiteBombMessage()
        {
        }

        public SpawnRandomizedSpiteBombMessage(float damage, Vector3 spawnPosition, GenericFireProjectileArgs genericArgs)
        {
            _damage = damage;
            _spawnPosition = spawnPosition;
            _genericArgs = genericArgs;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(_damage);
            writer.Write(_spawnPosition);
            writer.Write(_genericArgs);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _damage = reader.ReadSingle();
            _spawnPosition = reader.ReadVector3();
            _genericArgs = reader.Read<GenericFireProjectileArgs>();
        }

        public override void OnReceived()
        {
            const string LOG_PREFIX = $"{nameof(SpawnRandomizedSpiteBombMessage)}.{nameof(OnReceived)} ";

            if (!NetworkServer.active)
            {
#if DEBUG
                Log.Warning(LOG_PREFIX + "received on client");
#endif
                return;
            }

            BombArtifactManager.BombRequest bombRequest = new BombArtifactManager.BombRequest
            {
                spawnPosition = _spawnPosition,
                raycastOrigin = _spawnPosition + (UnityEngine.Random.insideUnitSphere * (BombArtifactManager.bombSpawnBaseRadius * BombArtifactManager.bombSpawnRadiusCoefficient)),
                bombBaseDamage = _damage,
                attacker = _genericArgs.Owner,
                teamIndex = _genericArgs.OwnerTeam,
                velocityY = UnityEngine.Random.Range(5f, 25f)
            };

            Ray ray = new Ray(bombRequest.raycastOrigin + new Vector3(0f, BombArtifactManager.maxBombStepUpDistance, 0f), Vector3.down);
            float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                SpiteBomb_SpawnHook.patchDisabledCount++;
                BombArtifactManager.SpawnBomb(bombRequest, raycastHit.point.y);
                SpiteBomb_SpawnHook.patchDisabledCount--;
            }
        }
    }
}
