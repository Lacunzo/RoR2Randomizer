using R2API.Networking;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.CharacterReplacements;
using RoR2Randomizer.Networking.EffectRandomizer;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Networking
{
    public static class CustomNetworkMessageManager
    {
        public static void RegisterMessages()
        {
            NetworkingAPI.RegisterMessageType<SyncBossReplacementCharacter>();
            NetworkingAPI.RegisterMessageType<SyncExplicitSpawnReplacement>();

            NetworkingAPI.RegisterMessageType<SyncProjectileReplacements>();
            NetworkingAPI.RegisterMessageType<SyncCharacterMasterReplacements>();
            NetworkingAPI.RegisterMessageType<SyncEffectReplacements>();

#if DEBUG
            NetworkingAPI.RegisterMessageType<Debug.SyncConsoleLog>();
#endif
        }
    }
}
