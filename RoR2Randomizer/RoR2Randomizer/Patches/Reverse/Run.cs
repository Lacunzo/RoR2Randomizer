using HarmonyLib;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RoR2Randomizer.Patches.Reverse
{
    [HarmonyPatch(typeof(RoR2.Run))]
    public static class Run
    {
        [HarmonyPatch(nameof(RoR2.Run.PickNextStageSceneFromCurrentSceneDestinations))]
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        public static void PickNextStageSceneFromSceneDestinations(RoR2.Run instance, SceneDef scene)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo SceneCatalog_get_mostRecentSceneDef_MI = AccessTools.PropertyGetter(typeof(SceneCatalog), nameof(SceneCatalog.mostRecentSceneDef));

                foreach (CodeInstruction instruction in instructions)
                {
                    if (instruction.Calls(SceneCatalog_get_mostRecentSceneDef_MI))
                    {
                        // Load arg1 (scene) instead of getting the most recent scene
                        instruction.opcode = OpCodes.Ldarg_1;
                        instruction.operand = null;
                    }

                    yield return instruction;
                }
            }

            _ = Transpiler(default);
        }
    }
}
