using EntityStates;
using HarmonyLib;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.Fixes
{
    // Some bodies don't have localPlayerAuthority set on the prefab (probably because they were never meant to be player controlled). This patch ensures it's always set to true, on both server and client, if the body is player controlled

    [PatchClass]
    static class PlayerCharacterBody_AuthorityFix
    {
        static readonly Hook NetworkIdentity_get_localPlayerAuthority_Hook = new Hook(AccessTools.DeclaredPropertyGetter(typeof(NetworkIdentity), nameof(NetworkIdentity.localPlayerAuthority)), static (Func<NetworkIdentity, bool> orig, NetworkIdentity self) =>
        {
            return NetworkIdentity_get_localPlayerAuthority(orig(self), self);
        }, new HookConfig { ManualApply = true });

        static readonly ILHook NetworkIdentity_OnStartServer_ILHook = new ILHook(AccessTools.DeclaredMethod(typeof(NetworkIdentity), nameof(NetworkIdentity.OnStartServer)), static il =>
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                                 x => x.MatchLdarg(0),
                                 x => x.MatchLdfld<NetworkIdentity>(nameof(NetworkIdentity.m_LocalPlayerAuthority))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => NetworkIdentity_get_localPlayerAuthority(default, default)));
            }
        }, new ILHookConfig { ManualApply = true });

        static void Apply()
        {
            NetworkIdentity_get_localPlayerAuthority_Hook.Apply();
        }

        static void Cleanup()
        {
            NetworkIdentity_get_localPlayerAuthority_Hook.Undo();
        }

        static bool NetworkIdentity_get_localPlayerAuthority(bool localPlayerAuthority, NetworkIdentity self)
        {
            if (!localPlayerAuthority)
            {
                if (ExplicitSpawnRandomizerController.IsActive)
                {
                    if (self.TryGetComponent(out CharacterBody body))
                    {
                        GameObject masterObject = body.masterObject;
                        if (masterObject && masterObject.GetComponent<PlayerCharacterMasterController>())
                        {
                            self.localPlayerAuthority = true;
                            return true;
                        }
                    }
                }
            }

            return localPlayerAuthority;
        }
    }
}
