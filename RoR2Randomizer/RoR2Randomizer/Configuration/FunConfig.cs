using BepInEx.Configuration;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System.Linq;

namespace RoR2Randomizer.Configuration
{
    public sealed class FunConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        readonly BoolConfigValue _gupMode;
        public bool GupModeActive => _gupMode && Caches.DLC.SOTV && EntitlementManager.localUserEntitlementTracker.AnyUserHasEntitlement(Caches.DLC.SOTV.requiredEntitlement);

        public FunConfig(ConfigFile file) : base("Fun", file)
        {
            _gupMode = new BoolConfigValue(getEntry("Gup Mode", "GUP ".Repeat(2500), false));
        }
    }
}
