using R2API.Networking;
using RoR2Randomizer.Networking.BossRandomizer;
#if DEBUG
using RoR2Randomizer.Networking.Debug;
#endif
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
            NetworkingAPI.RegisterMessageType<SyncProjectileReplacements>();

#if DEBUG
            NetworkingAPI.RegisterMessageType<SyncConsoleLog>();
#endif
        }
    }
}
