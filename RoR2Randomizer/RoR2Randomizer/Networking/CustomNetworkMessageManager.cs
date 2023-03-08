using R2API.Networking;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.CharacterReplacements;
using RoR2Randomizer.Networking.DamageOrbTargetDummy;
using RoR2Randomizer.Networking.EffectRandomizer;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Networking.Generic.Chunking;
using RoR2Randomizer.Networking.ItemRandomizer;
#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using RoR2Randomizer.Networking.HoldoutZoneRandomizer;
#endif
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.Networking.ProjectileRandomizer.Bullet;
using RoR2Randomizer.Networking.ProjectileRandomizer.Orbs;
using RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.GenericDamage;
using RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.Lightning;
using RoR2Randomizer.Networking.ProjectileRandomizer.SpiteBomb;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
using RoR2Randomizer.Networking.SurvivorPodRandomizer;

namespace RoR2Randomizer.Networking
{
    public static class CustomNetworkMessageManager
    {
        public static void RegisterMessages()
        {
            NetworkingAPI.RegisterMessageType<ChunkedNetworkMessage.ChunkedMessageHeader>();
            NetworkingAPI.RegisterMessageType<ChunkedNetworkMessage.MessageChunk>();

            NetworkingAPI.RegisterMessageType<SyncBossReplacementCharacter>();

            NetworkingAPI.RegisterMessageType<SyncExplicitSpawnReplacement>();
            NetworkingAPI.RegisterMessageType<SyncExplicitSpawnRandomizerEnabled>();

            NetworkingAPI.RegisterMessageType<SyncCharacterMasterReplacements>();
            NetworkingAPI.RegisterMessageType<SyncEffectReplacements>();

            NetworkingAPI.RegisterMessageType<SyncProjectileReplacements>();

            NetworkingAPI.RegisterMessageType<SyncBulletAttackCatalog>();
            NetworkingAPI.RegisterMessageType<SyncBulletAttackIndexNeeded>();

            NetworkingAPI.RegisterMessageType<SyncDamageOrbCatalog>();
            NetworkingAPI.RegisterMessageType<SyncDamageOrbIndexNeeded>();

            NetworkingAPI.RegisterMessageType<SyncLightningOrbCatalog>();
            NetworkingAPI.RegisterMessageType<SyncLightningOrbIndexNeeded>();

            NetworkingAPI.RegisterMessageType<ClientRequestOrbTargetMarkerObjects>();
            NetworkingAPI.RegisterMessageType<ClientRequestOrbTargetMarkerObjects.Reply>();

            NetworkingAPI.RegisterMessageType<SpawnRandomizedOrbMessage>();
            NetworkingAPI.RegisterMessageType<SpawnRandomizedSpiteBombMessage>();

            NetworkingAPI.RegisterMessageType<SyncSniperWeakPointReplacements>();

            NetworkingAPI.RegisterMessageType<SyncCharacterMasterReplacementMode>();

            NetworkingAPI.RegisterMessageType<SyncSurvivorPodReplacements>();

#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
            NetworkingAPI.RegisterMessageType<SyncHoldoutZoneReplacements>();
#endif

            NetworkingAPI.RegisterMessageType<SyncItemReplacements>();

#if DEBUG
            NetworkingAPI.RegisterMessageType<Debug.SyncConsoleLog>();
#endif
        }
    }
}
