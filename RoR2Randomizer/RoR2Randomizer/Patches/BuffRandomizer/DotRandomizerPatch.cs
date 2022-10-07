using RoR2;
using RoR2Randomizer.RandomizerControllers.Buff;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    static class DotRandomizerPatch
    {
        public static uint SkipApplyBuffCount = 0;

        static void Apply()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
        }

        static void Cleanup()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo -= DotController_InflictDot_refInflictDotInfo;
        }

        static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            if (BuffRandomizerController.IsActive &&
                SkipApplyBuffCount == 0 &&
                BuffRandomizerController.TryGetBuffIndex(inflictDotInfo.dotIndex, out BuffIndex buff) &&
                BuffRandomizerController.TryReplaceBuffIndex(ref buff))
            {
                if (inflictDotInfo.victimObject && inflictDotInfo.victimObject.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
                {
                    if (healthComponent.alive)
                    {
                        if (healthComponent.body)
                        {
                            BuffIndexPatch.SkipApplyDotCount++;
                            healthComponent.body.AddBuff(buff);
                            BuffIndexPatch.SkipApplyDotCount--;
                        }
                        else
                        {
                            Log.Warning($"{nameof(DotRandomizerPatch)} {nameof(DotController_InflictDot_refInflictDotInfo)} victim object has no body ({inflictDotInfo.victimObject})");
                        }
                    }
                }
                else
                {
                    Log.Warning($"{nameof(DotRandomizerPatch)} {nameof(DotController_InflictDot_refInflictDotInfo)} victim object has no health component ({inflictDotInfo.victimObject})");
                }

                if (BuffRandomizerController.TryGetDotIndex(buff, out DotController.DotIndex overrideDot))
                {
                    inflictDotInfo.dotIndex = overrideDot;
                }
                else
                {
                    return;
                }
            }

            orig(ref inflictDotInfo);
        }
    }
}
