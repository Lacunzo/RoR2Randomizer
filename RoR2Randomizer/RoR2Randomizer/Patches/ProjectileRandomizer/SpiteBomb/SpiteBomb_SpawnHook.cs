using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2.Artifacts;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.SpiteBomb
{
    [PatchClass]
    static class SpiteBomb_SpawnHook
    {
        internal static int patchDisabledCount = 0;

        static readonly Hook BombArtifactManager_SpawnBomb_Hook = new Hook(SymbolExtensions.GetMethodInfo(() => BombArtifactManager.SpawnBomb(default, default)), (Action<BombArtifactManager.BombRequest, float> orig, BombArtifactManager.BombRequest bombRequest, float groundY) =>
        {
            if (patchDisabledCount <= 0 && ProjectileRandomizerController.TryReplaceFire(bombRequest))
            {
                return;
            }
            else
            {
                orig(bombRequest, groundY);
            }
        }, new HookConfig { ManualApply = true });

        static void Apply()
        {
            BombArtifactManager_SpawnBomb_Hook.Apply();
        }

        static void Cleanup()
        {
            BombArtifactManager_SpawnBomb_Hook.Undo();
        }
    }
}
