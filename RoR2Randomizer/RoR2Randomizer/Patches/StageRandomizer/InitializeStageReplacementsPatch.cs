using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.RandomizerControllers.Stage;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    [PatchClass]
    public static class InitializeStageReplacementsPatch
    {
        static void Apply()
        {
            IL.RoR2.Run.Start += Run_Start;
        }

        static void Cleanup()
        {
            IL.RoR2.Run.Start -= Run_Start;
        }

        static void Run_Start(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(InitializeStageReplacementsPatch)}.{nameof(Run_Start)} ";

            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCallvirt<NetworkManager>(nameof(NetworkManager.ServerChangeScene))))
            {
                c.Emit(OpCodes.Dup); // Duplicate scene name
                c.Emit<StageRandomizerController>(OpCodes.Call, nameof(StageRandomizerController.InitializeStageReplacements));
            }
            else
            {
                Log.Warning(LOG_PREFIX + "unable to find patch location");
            }
        }
    }
}
