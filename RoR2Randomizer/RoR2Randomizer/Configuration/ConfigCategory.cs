using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityModdingUtility;

namespace RoR2Randomizer.Configuration
{
    public class ConfigCategory
    {
        static readonly InitializeOnAccessDictionary<Type, FieldInfo[]> _configFields = new InitializeOnAccessDictionary<Type, FieldInfo[]>(type =>
        {
            return ReflectionUtils.GetTypeHierarchyList(type).SelectMany(t =>
            {
                if (t == type)
                {
                    return t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                            .Where(f => Array.IndexOf(f.FieldType.GetInterfaces(), typeof(IConfigModCompatibility)) != -1);
                }
                else
                {
                    return _configFields[t];
                }
            }).ToArray();
        });

        public readonly string CategoryName;

        protected readonly ConfigFile _file;

        public virtual ModCompatibilityFlags CompatibilityFlags => ModCompatibilityFlags.None;

        public ConfigCategory(string categoryName, ConfigFile file)
        {
            CategoryName = categoryName;
            _file = file;
        }

        public void RunModCompatibilities()
        {
            ModCompatibilityFlags flags = CompatibilityFlags;

            if ((flags & ModCompatibilityFlags.RiskOfOptions) != 0 && ModCompatibility.RiskOfOptionsCompat.IsEnabled)
                riskOfOptionsCompatibility();
        }

        void riskOfOptionsCompatibility()
        {
            foreach (FieldInfo field in _configFields[GetType()])
            {
                ((IConfigModCompatibility)field.GetValue(this))?.CreateRiskOfOptionsEntry();
            }
        }

        protected ConfigEntry<T> getEntry<T>(string key, T defaultValue)
        {
            return getEntry(key, ConfigDescription.Empty, defaultValue);
        }

        protected ConfigEntry<T> getEntry<T>(string key, string description, T defaultValue)
        {
            return getEntry(key, new ConfigDescription(description), defaultValue);
        }

        protected ConfigEntry<T> getEntry<T>(string key, ConfigDescription description, T defaultValue)
        {
            return _file.Bind(new ConfigDefinition(CategoryName, key), defaultValue, description);
        }
    }
}
