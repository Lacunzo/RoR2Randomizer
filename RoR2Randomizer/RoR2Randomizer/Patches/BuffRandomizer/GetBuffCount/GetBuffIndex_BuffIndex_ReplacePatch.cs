using HarmonyLib;
using RoR2Randomizer.RandomizerControllers.Buff;
using System.Reflection;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    static partial class GetBuffIndex_BuffIndex_ReplacePatch
    {
        public static bool ForceDisable = false;

        static bool _patchEnabled = false;

        static readonly MethodInfo enablePatch_MI = SymbolExtensions.GetMethodInfo(() => enablePatch());
        static void enablePatch()
        {
            if (_patchEnabled)
            {
                Log.Warning("Already enabled!");
            }

            _patchEnabled = true;
        }

        static readonly MethodInfo disablePatch_MI = SymbolExtensions.GetMethodInfo(() => decrementEnabledCount());
        static void decrementEnabledCount()
        {
            if (!_patchEnabled)
            {
                Log.Warning("Already disabled!");
            }

            _patchEnabled = false;
        }

        static void Apply()
        {
            On.RoR2.CharacterBody.GetBuffCount_BuffIndex += CharacterBody_GetBuffCount_BuffIndex;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterBody.GetBuffCount_BuffIndex -= CharacterBody_GetBuffCount_BuffIndex;
        }

        static int CharacterBody_GetBuffCount_BuffIndex(On.RoR2.CharacterBody.orig_GetBuffCount_BuffIndex orig, RoR2.CharacterBody self, RoR2.BuffIndex buffType)
        {
            if (_patchEnabled && !ForceDisable)
            {
#if DEBUG
                BuffRandomizerController.SuppressBuffReplacementLogCount++;
#endif

                BuffRandomizerController.TryReplaceBuffIndex(ref buffType);

#if DEBUG
                BuffRandomizerController.SuppressBuffReplacementLogCount--;
#endif
            }

            return orig(self, buffType);
        }
    }
}
