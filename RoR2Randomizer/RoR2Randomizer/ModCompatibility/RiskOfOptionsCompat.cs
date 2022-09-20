using BepInEx.Bootstrap;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.ModCompatibility
{
    public static class RiskOfOptionsCompat
    {
        public static readonly InitializeOnAccess<bool> IsEnabled = new InitializeOnAccess<bool>(() => Chainloader.PluginInfos.ContainsKey(Constants.RISK_OF_OPTIONS_GUID));

        public static void Setup()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("A Risk of Rain 2 randomizer mod.");
        }
    }
}
