using EntityStates;
using EntityStates.BrotherMonster;
using R2API;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    [RandomizerController]
    public partial class BossRandomizerController : MonoBehaviour
    {
        public static bool IsReplacedBossCharacter(GameObject masterObject)
        {
            return masterObject && masterObject.GetComponent<BaseBossReplacement>();
        }

        void Awake()
        {
            Mithrix.Initialize();
            Voidling.Initialize();
            Aurelionite.Initialize();
            LunarScav.Initialize();
            AlloyWorshipUnit.Initialize();
        }

        void OnDestroy()
        {
            Mithrix.Uninitialize();
            Voidling.Uninitialize();
            Aurelionite.Uninitialize();
            LunarScav.Uninitialize();
            AlloyWorshipUnit.Uninitialize();
        }
    }
}
