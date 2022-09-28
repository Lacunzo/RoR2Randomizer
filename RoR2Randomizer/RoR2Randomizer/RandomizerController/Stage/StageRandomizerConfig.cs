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

        const float FIRST_STAGE_WEIGHT_MULT_DEFAULT = 0.35f;
        public readonly SliderConfigValue<float> PossibleFirstStageWeightMult;

        public StageRandomizerConfig(ConfigFile file) : base("Stage", file)
        {
            FirstStageBlacklistEnabled = new BoolConfigValue(getEntry("Starting Stage Blacklist", "Ensures the first stage is always normal(ish) (No Commencement, or Voidling fight on stage 1)", true));

            PossibleFirstStageWeightMult = new SliderConfigValue<float>(getEntry("Normal starting stage weight multiplier", $"If set to 0.50: Stages that have a chance to be selected as the first stage in a run are half as likely to get picked as the first stage by the stage randomizer.\n\nIf set to 0.25: Starting stages have a 4 times smaller chance to get picked as the first stage.\n\nIf set to 1.00: Feature is effectively disabled, and all stages will be equally likely to get picked.\n\nDefault value: {FIRST_STAGE_WEIGHT_MULT_DEFAULT:F2}", FIRST_STAGE_WEIGHT_MULT_DEFAULT), SliderType.StepSlider, 0f, 1.5f, "{0:F2}", 0.05f);
        }
    }
}
