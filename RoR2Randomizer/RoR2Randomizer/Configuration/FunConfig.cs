using BepInEx.Configuration;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public sealed class FunConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        static ExpansionDef DLC1;

        [SystemInitializer(typeof(ExpansionCatalog))]
        static void Init()
        {
            const string DLC1_NAME = "DLC1";
            DLC1 = ExpansionCatalog.expansionDefs.FirstOrDefault(static e => e.name == DLC1_NAME);
#if DEBUG
            if (!DLC1) Log.Warning($"Unable to find {nameof(ExpansionDef)} {DLC1_NAME}");
#endif
        }

        readonly BoolConfigValue _gupMode;
        public bool GupModeActive => _gupMode && DLC1 && EntitlementManager.localUserEntitlementTracker.AnyUserHasEntitlement(DLC1.requiredEntitlement);

        public FunConfig(ConfigFile file) : base("Fun", file)
        {
            _gupMode = new BoolConfigValue(getEntry("Gup Mode", "GUP ".Repeat(2500), false));
        }
    }
}
