using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer.Heretic
{
    [PatchClass]
    static class Heretic_TransformBodyHook
    {
        static void Apply()
        {
            IL.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.CharacterMaster.TransformBody += CharacterMaster_TransformBody;
        }

        static void Cleanup()
        {
            IL.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
            On.RoR2.CharacterMaster.TransformBody -= CharacterMaster_TransformBody;
        }

        static void CharacterMaster_OnInventoryChanged(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After, x => x.MatchLdstr(Constants.BodyNames.HERETIC_NAME)))
            {
                c.EmitDelegate(static (string bodyName) =>
                {
                    if (ExplicitSpawnRandomizerController.IsActive)
                    {
                        if (ExplicitSpawnRandomizerController.TryGetReplacementBodyName(bodyName, out string replacementName))
                        {
                            return replacementName;
                        }
                    }

                    return bodyName;
                });
            }
        }

        static void CharacterMaster_TransformBody(On.RoR2.CharacterMaster.orig_TransformBody orig, CharacterMaster self, string bodyName)
        {
            if (ExplicitSpawnRandomizerController.IsActive && !self.GetComponent<ExplicitSpawnReplacementInfo>())
            {
                if (
#if DEBUG
                    ConfigManager.Debug.CharacterDebugMode.Entry.Value > DebugMode.None ||
#endif
                    CharacterReplacements.IsAnyForcedCharacterModeEnabled ||
                    (ExplicitSpawnRandomizerController.TryGetOriginalBodyName(bodyName, out string originalBody) &&
                     originalBody == Constants.BodyNames.HERETIC_NAME))
                {
                    ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(self.gameObject, Caches.Masters.Heretic);
                }
            }

            orig(self, bodyName);
        }
    }
}
