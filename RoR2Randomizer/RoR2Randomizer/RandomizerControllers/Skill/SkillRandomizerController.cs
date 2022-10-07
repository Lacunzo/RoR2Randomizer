#if !DISABLE_SKILL_RANDOMIZER
// #define MANUAL_SKILL_INDEX

using RoR2;
using RoR2.Skills;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2Randomizer.RandomizerController.Skill
{
    [RandomizerController]
    public class SkillRandomizerController : MonoBehaviour
    {
#if MANUAL_SKILL_INDEX
        static int _debugSkillIndex = 0;
#endif

        static List<SkillFamily.Variant> _availableSkills;
        static List<SkillLocator.PassiveSkill> _availablePassiveSkills;

        void Update()
        {
            if (_availableSkills == null && BodyCatalog.skillSlots != null)
            {
                _availableSkills = new List<SkillFamily.Variant>();
                _availablePassiveSkills = new List<SkillLocator.PassiveSkill>();

                int c = 0;
                for (int i = 0; i < BodyCatalog.skillSlots.Length; i++)
                {
                    foreach (GenericSkill skills in BodyCatalog.skillSlots[i])
                    {
                        if (skills.skillFamily)
                        {
                            foreach (SkillFamily.Variant variant in skills.skillFamily.variants)
                            {
                                Type stateType = variant.skillDef.activationState.stateType;
                                if (stateType != null
                                    && stateType != typeof(EntityStates.Idle)
                                    && stateType != typeof(EntityStates.AncientWispMonster.Throw) // Fires a projectile that does nothing
                                    && stateType != typeof(EntityStates.ArchWispMonster.ChargeCannons) // Duplicate of GreaterWispMonster.ChargeCannons
                                    && stateType != typeof(EntityStates.ImpMonster.FireSpines) // Fires a projectile that does nothing
#if true
                                    // requires more than a little bit of work to function on any character, but should be properly attempted at some point
                                    && stateType != typeof(EntityStates.Assassin2.ChargeDash) // NullRef when used by non-ai, relies on animator states, requires hitboxgroups
                                    && stateType != typeof(EntityStates.Assassin.Weapon.SlashCombo) // relies on animator states
                                    && stateType != typeof(EntityStates.Bandit2.Weapon.SlashBlade) // requires a hitboxgroup
#endif
                                                      )
                                {
                                    if (!_availableSkills.Any(v => v.skillDef == variant.skillDef))
                                    {
                                        _availableSkills.Add(variant);
                                        Log.Debug($"{c++}: skill: [{variant.ToLogString()}]");
                                    }
                                }
                            }
                        }
                    }

                    CharacterBody body = BodyCatalog.bodyPrefabBodyComponents[i];
                    if (body)
                    {
                        SkillLocator skillLocator = body.GetComponent<SkillLocator>();
                        if (skillLocator)
                        {
                            SkillLocator.PassiveSkill passive = skillLocator.passiveSkill;
                            if (passive.enabled)
                            {
                                _availablePassiveSkills.Add(passive);
                                Log.Debug($"{body.GetDisplayName()} passive:  skillNameToken: {passive.skillNameToken}, skillDescriptionToken: {passive.skillDescriptionToken}, keywordToken: {passive.keywordToken}");
                            }
                        }
                    }
                }
            }

#if MANUAL_SKILL_INDEX
            if (Main.SKILL_RANDOMIZER_ENABLED)
            {
                bool skillIndexChanged = false;

                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_debugSkillIndex >= _availableSkills.Count)
                        _debugSkillIndex = 0;

                    skillIndexChanged = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_debugSkillIndex < 0)
                        _debugSkillIndex = _availableSkills.Count - 1;

                    skillIndexChanged = true;
                }

                if (skillIndexChanged)
                {
                    Chat.AddMessage($"{nameof(_debugSkillIndex)}: {_debugSkillIndex} ({_availableSkills[_debugSkillIndex].ToLogString()})");
                }
            }
#endif
        }

        public static void RandomizeSkill(GenericSkill self, CharacterBody body)
        {
            if (!ConfigManager.SkillRandomizer.Enabled)
                return;

            SkillFamily.Variant[] variants = self.skillFamily.variants;
            for (int i = 0; i < variants.Length; i++)
            {
                SkillFamily.Variant variant = _availableSkills
#if MANUAL_SKILL_INDEX
                                                              [_debugSkillIndex];
#else
                                                              .GetRandomOrDefault();
#endif

#if DEBUG
                Log.Debug($"Replace {body.GetDisplayName()} skill[{i}]: {variants[i].ToLogString()} -> {variant.ToLogString()}");
#endif

                variants[i] = variant;
            }
        }
    }
}
#endif