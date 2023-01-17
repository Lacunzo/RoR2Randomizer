using BepInEx.Configuration;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public class SliderConfigValue<T> : GenericConfigValue<T>
    {
        readonly SliderType _type;
        readonly float _min;
        readonly float _max;
        readonly string _format;
        readonly float _increment;

        public SliderConfigValue(ConfigEntry<T> entry, SliderType type, float min, float max, string format, float increment) : base(entry)
        {
            _type = type;
            _min = min;
            _max = max;
            _format = string.IsNullOrEmpty(format) ? "{0}" : "{0:" + format + "}";
            _increment = increment;
        }

        public SliderConfigValue(ConfigEntry<T> entry, SliderType type, float min, float max, string format) : this(entry, type, min, max, format, 1f)
        {
        }

        public SliderConfigValue(ConfigEntry<T> entry, SliderType type, float min, float max) : this(entry, type, min, max, "F1")
        {
        }

        public override void CreateRiskOfOptionsEntry()
        {
            RiskOfOptions.Options.BaseOption option;
            switch (_type)
            {
                case SliderType.Slider:
                    option = new RiskOfOptions.Options.SliderOption((ConfigEntry<float>)(object)Entry, new RiskOfOptions.OptionConfigs.SliderConfig { min = _min, max = _max, formatString = _format });
                    break;
                case SliderType.IntSlider:
                    option = new RiskOfOptions.Options.IntSliderOption((ConfigEntry<int>)(object)Entry, new RiskOfOptions.OptionConfigs.IntSliderConfig { min = (int)_min, max = (int)_max, formatString = _format });
                    break;
                case SliderType.StepSlider:
                    option = new RiskOfOptions.Options.StepSliderOption((ConfigEntry<float>)(object)Entry, new RiskOfOptions.OptionConfigs.StepSliderConfig { min = _min, max = _max, formatString = _format, increment = _increment });
                    break;
                default:
                    Log.Warning($"({typeof(T).Name}) Slider type {_type} is not implemented!");
                    return;
            }

            RiskOfOptions.ModSettingsManager.AddOption(option);
        }
    }
}
