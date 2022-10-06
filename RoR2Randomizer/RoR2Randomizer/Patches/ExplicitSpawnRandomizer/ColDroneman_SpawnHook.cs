using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerController.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class ColDroneman_SpawnHook
    {
        static void Apply()
        {
            IL.RoR2.DroneWeaponsBehavior.TrySpawnDrone += DroneWeaponsBehavior_TrySpawnDrone;
        }

        static void Cleanup()
        {
            IL.RoR2.DroneWeaponsBehavior.TrySpawnDrone -= DroneWeaponsBehavior_TrySpawnDrone;
        }

        static void DroneWeaponsBehavior_TrySpawnDrone(ILContext il)
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
