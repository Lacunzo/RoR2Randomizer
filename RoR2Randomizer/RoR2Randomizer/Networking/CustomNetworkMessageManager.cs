﻿using R2API.Networking;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.Debug;
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
            NetworkingAPI.RegisterMessageType<SyncConsoleLog>();
        }
    }
}
