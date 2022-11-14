#if DEBUG
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.Networking;
using RoR2Randomizer.Configuration;
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

            On.RoR2.PlayerCharacterMasterControllerEntitlementTracker.HasEntitlement += PlayerCharacterMasterControllerEntitlementTracker_HasEntitlement;
        }

        static void Cleanup()
        {
            On.RoR2.Networking.NetworkManagerSystem.ClientSendAuth -= NetworkManagerSystem_ClientSendAuth;

            On.RoR2.PlayerCharacterMasterControllerEntitlementTracker.HasEntitlement -= PlayerCharacterMasterControllerEntitlementTracker_HasEntitlement;
        }

        static void NetworkManagerSystem_ClientSendAuth(On.RoR2.Networking.NetworkManagerSystem.orig_ClientSendAuth orig, NetworkManagerSystem self, NetworkConnection conn)
        {
            if (!ConfigManager.Debug.AllowLocalhostConnect)
            {
                orig(self, conn);
            }
        }

        // The client player does not have any entitlements when connected for some reason, so just force them enabled in that case
        static bool PlayerCharacterMasterControllerEntitlementTracker_HasEntitlement(On.RoR2.PlayerCharacterMasterControllerEntitlementTracker.orig_HasEntitlement orig, PlayerCharacterMasterControllerEntitlementTracker self, EntitlementDef entitlementDef)
        {
            if (ConfigManager.Debug.AllowLocalhostConnect)
            {
                return true;
            }
            else
            {
                return orig(self, entitlementDef);
            }
        }
    }
}
#endif