using BepInEx.Configuration;
using HG;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.Configuration.ConfigValue.ParsedList;
using System;
using System.Linq;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.Buff
{
    public sealed class BuffRandomizerConfig : BaseRandomizerConfig
    {
        public BoolConfigValue MixBuffsAndDebuffs;

        readonly SliderConfigValue<float> _swapBuffDebuffWeightMult;
        public float SwapBuffDebuffWeightMult => Mathf.Max(0f, _swapBuffDebuffWeightMult);

        public BoolConfigValue ExcludeInvincibility;

        public BoolConfigValue ExcludeEliteBuffs;

        readonly ParsedBuffIndexListConfigValue _buffBlacklist;

        public bool IsBlacklisted(BuffIndex buffIndex)
        {
            return _buffBlacklist.BinarySearch(buffIndex) >= 0;
        }

#if DEBUG
        public readonly EnumConfigValue<DebugMode> BuffDebugMode;

        public readonly StringConfigValue ForcedBuffName;
#endif

        public BuffRandomizerConfig(ConfigFile file) : base("Status Effect", file)
        {
            MixBuffsAndDebuffs = new BoolConfigValue(getEntry("Mix buffs and debuffs", "If the randomizer should be able to turn a buff (positive effect) into a debuff (negative effect) and vice versa.", true));

            _swapBuffDebuffWeightMult = new SliderConfigValue<float>(getEntry("Buff-Debuff Weight Multiplier", "The weight multiplier to apply to buffs randomizing into debuffs and vice verse.\n\nThe lower the value, the less likely a buff will be to get randomized into a debuff, and vice versa.", 0.6f), SliderType.StepSlider, 0f, 1f, "F2", 0.05f);

            ExcludeInvincibility = new BoolConfigValue(getEntry("Blacklist Invincibility Effects", "Disables randomization (both from and to) of status effects that make the user invincible. Turning this off can potentially softlock a run due to a boss becoming invincible.", true));

            ExcludeEliteBuffs = new BoolConfigValue(getEntry("Blacklist Elite aspects", "Disables randomization (both from and to) of status effects that are associated with an elite aspect", false));

            _buffBlacklist = new ParsedBuffIndexListConfigValue(getEntry("Status Effect Blacklist", "Comma separated list of status effects to exclude from the randomizer\n\nList of status effects: https://riskofrain2.fandom.com/wiki/Status_Effects (use the \"Internal Name\" of the effect for this value)\n\nExample value: \"ElementalRingVoidReady,BearVoidCooldown\" will make the randomizer not change Singularity Band Ready and Safer Spaces cooldown", string.Empty));

#if DEBUG
            BuffDebugMode = new EnumConfigValue<DebugMode>(getEntry("Debug Mode", DebugMode.None));
            ForcedBuffName = new StringConfigValue(getEntry("Forced Buff Name", string.Empty));
#endif
        }
    }
}
