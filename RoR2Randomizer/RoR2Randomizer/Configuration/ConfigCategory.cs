using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
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
            List<FieldInfo> fields = new List<FieldInfo>();

            do
            {
                fields.InsertRange(0, type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(f => f.FieldType.GetInterface(nameof(IConfigModCompatibility)) != null));
            } while ((type = type.BaseType) != null);

            return fields.ToArray();
        });

        public readonly string CategoryName;

        protected readonly ConfigFile _file;

        public ConfigCategory(string categoryName, ConfigFile file)
        {
            CategoryName = categoryName;
            _file = file;
        }

        public void RiskOfOptionsCompatibility()
        {
            foreach (FieldInfo field in _configFields[GetType()])
            {
                ((IConfigModCompatibility)field.GetValue(this)).CreateRiskOfOptionsEntry();
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
