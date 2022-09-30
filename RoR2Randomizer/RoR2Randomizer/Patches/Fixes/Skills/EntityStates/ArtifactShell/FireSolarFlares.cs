#if !DISABLE_SKILL_RANDOMIZER
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.ArtifactShell
{
    [PatchClass]
    public static class FireSolarFlares
    {
        static void Apply()
        {
            On.EntityStates.ArtifactShell.FireSolarFlares.OnEnter += FireSolarFlares_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.ArtifactShell.FireSolarFlares.OnEnter -= FireSolarFlares_OnEnter;
        }

        static int _origMinProjectileCount = -1;
        static int _origMaxProjectileCount = -1;

        static void recordMinMaxProjectileCountsIfNeeded()
        {
            if (_origMinProjectileCount == -1 || _origMaxProjectileCount == -1)
            {
                _origMinProjectileCount = global::EntityStates.ArtifactShell.FireSolarFlares.minimumProjectileCount;
                _origMaxProjectileCount = global::EntityStates.ArtifactShell.FireSolarFlares.maximumProjectileCount;
            }
        }

        static void overrideMinMaxProjectileCounts(int min, int max)
        {
            recordMinMaxProjectileCountsIfNeeded();

            global::EntityStates.ArtifactShell.FireSolarFlares.minimumProjectileCount = min;
            global::EntityStates.ArtifactShell.FireSolarFlares.maximumProjectileCount = max;
        }

        static void restoreMinMaxProjectileCounts()
        {
            global::EntityStates.ArtifactShell.FireSolarFlares.minimumProjectileCount = _origMinProjectileCount;
            global::EntityStates.ArtifactShell.FireSolarFlares.maximumProjectileCount = _origMaxProjectileCount;
        }

        static void FireSolarFlares_OnEnter(On.EntityStates.ArtifactShell.FireSolarFlares.orig_OnEnter orig, global::EntityStates.ArtifactShell.FireSolarFlares self)
        {
            bool isArtifactBoss = self.GetComponent<ArtifactTrialMissionController>();

            if (!isArtifactBoss)
                overrideMinMaxProjectileCounts(20, 5);

            orig(self);

            if (!isArtifactBoss)
                restoreMinMaxProjectileCounts();
        }
    }
}
#endif