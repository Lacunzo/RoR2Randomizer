using EntityStates;
using EntityStates.BrotherMonster;
using R2API;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public partial class BossRandomizerController : Singleton<BossRandomizerController>
    {
        public static bool IsReplacedBossCharacter(GameObject masterObject)
        {
            return masterObject && masterObject.GetComponent<BaseBossReplacement>();
        }

        protected override void Awake()
        {
            base.Awake();

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
