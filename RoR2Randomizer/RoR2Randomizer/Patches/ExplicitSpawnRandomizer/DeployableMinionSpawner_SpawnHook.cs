using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.RandomizerController.ExplicitSpawn;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class DeployableMinionSpawner_SpawnHook
    {
        static void Apply()
        {
            IL.RoR2.DeployableMinionSpawner.SpawnMinion += DeployableMinionSpawner_SpawnMinion;
        }

        static void DeployableMinionSpawner_SpawnMinion(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate(ExplicitSpawnRandomizerController.ReplaceDirectorSpawnRequest);
            }
        }
    }
}
