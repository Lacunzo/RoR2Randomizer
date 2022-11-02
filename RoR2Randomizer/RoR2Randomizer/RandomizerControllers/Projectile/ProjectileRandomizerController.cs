using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    [RandomizerController]
    public class ProjectileRandomizerController : BaseRandomizerController
    {
        static int[] _projectileIndicesToRandomize;

        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            _projectileIndicesToRandomize =
                ProjectileCatalog.projectilePrefabProjectileControllerComponents
                                 .Where(projectile =>
                                 {
                                     if (!projectile)
                                         return false;
                                     
                                     if (projectile.TryGetComponent<ProjectileFireChildren>(out ProjectileFireChildren projectileFireChildren)
                                      && (!projectileFireChildren.childProjectilePrefab || projectileFireChildren.childProjectilePrefab == null))
                                     {
#if DEBUG                            
                                         Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to invalid {nameof(ProjectileFireChildren)} setup");
#endif                               
                                     
                                         return false;
                                     }
                                     
                                     switch (projectile.name)
                                     {
                                         case "AACannon": // Does nothing
                                         case "AncientWispCannon": // Does nothing
                                         case "BanditBomblets": // Does nothing
                                         case "BanditClusterBombSeed": // Clusterbombs fall through ground and do nothing
                                         case "BanditClusterGrenadeProjectile": // No collision, cannot deal damage
                                         case "BeetleQueenAcid": // Does nothing
                                         case "BellBallSmall": // Does nothing
                                         case "DroneRocket": // Does nothing
                                         case "EngiMineDeployer": // Constant NullRef in FixedUpdate
                                         case "EngiSeekerGrenadeProjectile": // Does nothing
                                         case "EngiWallShield": // Unfinished engi shield
                                         case "GatewayProjectile": // Does nothing
                                         case "MinorConstructOnKillProjectile": // Does nothing
                                         case "NullifierBombProjectile": // Does nothing
                                         case "PaladinBigRocket": // Does nothing
                                         case "RedAffixMissileProjectile": // Does nothing
                                         case "ScoutGrenade": // Does nothing
                                         case "Rocket": // Does nothing
                                         case "Spine": // No collision, cannot deal damage
                                         case "ToolbotDroneHeal": // Does nothing
                                         case "ToolbotDroneStun": // Does nothing
                                         case "TreebotPounderProjectile": // Does nothing
                                     
                                         // Excluded because I think it's more fun that way
                                         case "MageIcewallWalkerProjectile":
                                         case "MageFirewallWalkerProjectile":
                                     
                                         // Excluded because it seems like a huge pain getting it to work, might look into it in the future.
                                         case "LunarSunProjectile":
#if DEBUG                            
                                             Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to being in blacklist");
#endif                               
                                             return false;
                                     }
                                     
                                     return true;
                                 })
                                 .Select(p => p.catalogIndex)
                                 .ToArray();
        }

        static readonly RunSpecific<bool> _hasReceivedProjectileReplacementsFromServer = new RunSpecific<bool>();

        static readonly RunSpecific<IndexReplacementsCollection> _projectileIndicesReplacements = new RunSpecific<IndexReplacementsCollection>((out IndexReplacementsCollection result) =>
        {
            if (shouldBeActive)
            {
                ReplacementDictionary<int> dict = ReplacementDictionary<int>.CreateFrom(_projectileIndicesToRandomize);

                result = new IndexReplacementsCollection(dict, ProjectileCatalog.projectilePrefabCount);

                return true;
            }

            result = default;
            return false;
        });

        static bool shouldBeActive => NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled;
        public static bool IsActive => (shouldBeActive || (NetworkClient.active && _hasReceivedProjectileReplacementsFromServer)) && _projectileIndicesReplacements.HasValue;

        public override bool IsRandomizerEnabled => IsActive;

        protected override bool isNetworked => true;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
#if DEBUG
            Log.Debug($"Sending {nameof(SyncProjectileReplacements)} to clients");
#endif

            yield return new SyncProjectileReplacements(_projectileIndicesReplacements);
        }

        static void onProjectileReplacementsReceivedFromServer(IndexReplacementsCollection replacements)
        {
            _projectileIndicesReplacements.Value = replacements;
            _hasReceivedProjectileReplacementsFromServer.Value = true;
        }

        protected override void Awake()
        {
            base.Awake();

            SyncProjectileReplacements.OnReceive += onProjectileReplacementsReceivedFromServer;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SyncProjectileReplacements.OnReceive -= onProjectileReplacementsReceivedFromServer;

            _projectileIndicesReplacements.Dispose();
            _hasReceivedProjectileReplacementsFromServer.Dispose();
        }

#if DEBUG
        static bool getDebugProjectileReplacement(int original, out int replacement)
        {
            if (_projectileIndicesReplacements.Value.HasReplacement(original))
            {
                switch (ConfigManager.ProjectileRandomizer.DebugMode.Entry.Value)
                {
                    case DebugMode.Manual:
                        replacement = _projectileIndicesToRandomize[_forcedProjectileIndex];
                        return true;
                    case DebugMode.Forced:
                        return (replacement = ConfigManager.ProjectileRandomizer.ForcedProjectileIndex.Parsed) >= 0;
                }
            }

            replacement = -1;
            return false;
        }
#endif

        public static void TryOverrideProjectilePrefab(ref GameObject prefab)
        {
            if (IsActive)
            {
                int originalIndex = ProjectileCatalog.GetProjectileIndex(prefab);
                if (originalIndex != -1 && TryGetOverrideProjectileIndex(originalIndex, out int replacementIndex))
                {
                    GameObject replacementPrefab = ProjectileCatalog.GetProjectilePrefab(replacementIndex);
                    if (replacementPrefab)
                    {
#if DEBUG
                        Log.Debug($"Projectile randomizer: Replaced projectile: {prefab.name} ({originalIndex}) -> {replacementPrefab.name} ({replacementIndex})");
#endif

                        prefab = replacementPrefab;
                    }
                }
            }
        }

        public static bool TryGetOverrideProjectileIndex(int originalIndex, out int replacementIndex)
        {
            if (IsActive)
            {
                if (
#if DEBUG
                    getDebugProjectileReplacement(originalIndex, out replacementIndex) ||
#endif
                    _projectileIndicesReplacements.Value.TryGetReplacement(originalIndex, out replacementIndex))
                {
                    return true;
                }
            }

            replacementIndex = -1;
            return false;
        }

        public static bool TryGetOriginalProjectileIndex(int replacementIndex, out int originalIndex)
        {
            if (IsActive && _projectileIndicesReplacements.Value.TryGetOriginal(replacementIndex, out originalIndex))
            {
                return true;
            }

            originalIndex = -1;
            return false;
        }

        public static bool TryGetOriginalProjectilePrefab(GameObject replacementPrefab, out GameObject originalPrefab)
        {
            if (TryGetOriginalProjectileIndex(ProjectileCatalog.GetProjectileIndex(replacementPrefab), out int originalIndex))
            {
                originalPrefab = ProjectileCatalog.GetProjectilePrefab(originalIndex);
                return (bool)originalPrefab;
            }

            originalPrefab = null;
            return false;
        }

#if DEBUG
        static int _forcedProjectileIndex = 0;

        void Update()
        {
            if (ConfigManager.ProjectileRandomizer.Enabled && ConfigManager.ProjectileRandomizer.DebugMode == DebugMode.Manual)
            {
                bool changedProjectileIndex = false;
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_forcedProjectileIndex >= _projectileIndicesToRandomize.Length)
                            _forcedProjectileIndex = 0;

                    changedProjectileIndex = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                        if (--_forcedProjectileIndex < 0)
                        _forcedProjectileIndex = _projectileIndicesToRandomize.Length - 1;

                    changedProjectileIndex = true;
                }

                if (changedProjectileIndex)
                {
                    Log.Debug($"Current projectile override: {ProjectileCatalog.GetProjectilePrefab(_projectileIndicesToRandomize[_forcedProjectileIndex]).name} ({_projectileIndicesToRandomize[_forcedProjectileIndex]})");
                }
            }
        }
#endif
    }
}
