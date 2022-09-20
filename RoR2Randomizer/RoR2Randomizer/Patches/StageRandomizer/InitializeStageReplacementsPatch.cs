using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.RandomizerController.Stage;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    public static class InitializeStageReplacementsPatch
    {
        public static void Apply()
        {
            IL.RoR2.Run.Start += Run_Start;
        }

        public static void Cleanup()
        {
            IL.RoR2.Run.Start -= Run_Start;
        }

        static void Run_Start(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCallvirt<NetworkManager>(nameof(NetworkManager.ServerChangeScene))))
            {
                c.Emit(OpCodes.Dup); // Duplicate scene name
                c.Emit<StageRandomizerController>(OpCodes.Call, nameof(StageRandomizerController.InitializeStageReplacements));
            }
        }
    }
}
