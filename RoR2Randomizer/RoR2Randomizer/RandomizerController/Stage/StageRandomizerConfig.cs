using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Stage
{
    public class StageRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue FirstStageBlacklistEnabled;

        public StageRandomizerConfig(ConfigFile file) : base("Stage Randomizer", file)
        {
            FirstStageBlacklistEnabled = new BoolConfigValue(getEntry<bool>("Starting Stage Blacklist", "Ensures the first stage is always normal(ish) (No obliteration, Commencement, or Voidling fight on stage 1)", true));
        }
    }
}
