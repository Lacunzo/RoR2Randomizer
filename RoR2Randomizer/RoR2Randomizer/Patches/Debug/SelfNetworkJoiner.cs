#if DEBUG
using RoR2.Networking;
using RoR2Randomizer.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.Debug
{
    [PatchClass]
    static class SelfNetworkJoiner
    {
        // "Borrowed" from: https://github.com/niwith/DropInMultiplayer/blob/master/DropInMultiplayerV2/Main.cs#L90
        // This is so we can connect to ourselves.
        // Instructions:
        // Step One: Start two instances of RoR2 (do this through the .exe directly)
        // Step Two: Host a game with one instance of RoR2.
        // Step Three: On the instance that isn't hosting, open up the console (ctrl + alt + tilde) and enter the command "connect localhost:7777"
        // DO NOT MAKE A MISTAKE SPELLING THE COMMAND OR YOU WILL HAVE TO RESTART THE CLIENT INSTANCE!!
        // Step Four: Test whatever you were going to test.

        static void Apply()
        {
            On.RoR2.Networking.NetworkManagerSystem.ClientSendAuth += NetworkManagerSystem_ClientSendAuth;
        }

        static void Cleanup()
        {
            On.RoR2.Networking.NetworkManagerSystem.ClientSendAuth -= NetworkManagerSystem_ClientSendAuth;
        }

        static void NetworkManagerSystem_ClientSendAuth(On.RoR2.Networking.NetworkManagerSystem.orig_ClientSendAuth orig, NetworkManagerSystem self, NetworkConnection conn)
        {
            if (!ConfigManager.Debug.AllowSelfNetworkJoin)
            {
                orig(self, conn);
            }
        }
    }
}
#endif