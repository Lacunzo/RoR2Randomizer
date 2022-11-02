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
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    [RandomizerController]
    public partial class BossRandomizerController : BaseRandomizerController
    {
        public static bool IsReplacedBossCharacter(GameObject masterObject)
        {
            return masterObject && masterObject.GetComponent<BaseBossReplacement>();
        }

        public override bool IsRandomizerEnabled => NetworkServer.active && (ConfigManager.BossRandomizer.Enabled || CharacterReplacements.IsAnyForcedCharacterModeEnabled);

        protected override bool isNetworked => false;

        static BossRandomizerController _instance;

        protected override void Awake()
        {
            base.Awake();

            SingletonHelper.Assign(ref _instance, this);

            Mithrix.Initialize();
            Voidling.Initialize();
            Aurelionite.Initialize();
            LunarScav.Initialize();
            AlloyWorshipUnit.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Mithrix.Uninitialize();
            Voidling.Uninitialize();
            Aurelionite.Uninitialize();
            LunarScav.Uninitialize();
            AlloyWorshipUnit.Uninitialize();

            SingletonHelper.Unassign(ref _instance, this);
        }
    }
}
