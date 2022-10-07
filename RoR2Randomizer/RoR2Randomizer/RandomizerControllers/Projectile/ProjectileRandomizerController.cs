using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.Utility;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    [RandomizerController]
    public class ProjectileRandomizerController : MonoBehaviour
    {
        static readonly InitializeOnAccess<int[]> _projectileIndicesToRandomize = new InitializeOnAccess<int[]>(() =>
        {
            return ProjectileCatalog.projectilePrefabProjectileControllerComponents
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
        });

        static readonly RunSpecific<bool> _hasReceivedProjectileReplacementsFromServer = new RunSpecific<bool>();

        static readonly RunSpecific<ReplacementDictionary<int>> _projectileIndicesReplacements = new RunSpecific<ReplacementDictionary<int>>((out ReplacementDictionary<int> result) =>
        {
            if (NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled)
            {
                result = ReplacementDictionary<int>.CreateFrom(_projectileIndicesToRandomize.Get);

#if DEBUG
                Log.Debug($"Sending {nameof(SyncProjectileReplacements)} to clients");
#endif

                new SyncProjectileReplacements(result).Send(NetworkDestination.Clients);

                return true;
            }

            result = default;
            return false;
        });

        static bool shouldBeActive => (NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled) || _hasReceivedProjectileReplacementsFromServer;

        public static void OnProjectileReplacementsReceivedFromServer(ReplacementDictionary<int> replacements)
        {
            _projectileIndicesReplacements.Value = replacements;
            _hasReceivedProjectileReplacementsFromServer.Value = true;
        }

        void OnDestroy()
        {
            _projectileIndicesReplacements.Dispose();
        }

#if DEBUG
        static bool getDebugProjectileReplacement(int original, out int replacement)
        {
            if (_projectileIndicesReplacements.Value.HasReplacement(original))
            {
                switch (ConfigManager.ProjectileRandomizer.DebugMode.Entry.Value)
                {
                    case DebugMode.Manual:
                        replacement = _projectileIndicesToRandomize.Get[_forcedProjectileIndex];
                        return true;
                    case DebugMode.Forced:
                        return int.TryParse(ConfigManager.ProjectileRandomizer.ForcedProjectileIndex.Entry.Value.Trim(), out replacement);
                }
            }

            replacement = -1;
            return false;
        }
#endif

        public static void TryOverrideProjectilePrefab(ref GameObject prefab)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue)
            {
                int originalIndex = ProjectileCatalog.GetProjectileIndex(prefab);
                int replacementIndex;
                if (
#if DEBUG
                    getDebugProjectileReplacement(originalIndex, out replacementIndex) ||
#endif
                    _projectileIndicesReplacements.Value.TryGetReplacement(originalIndex, out replacementIndex))
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

        public static bool TryGetOriginalProjectileIndex(int replacementIndex, out int originalIndex)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue && _projectileIndicesReplacements.Value.TryGetOriginal(replacementIndex, out originalIndex))
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
                    if (++_forcedProjectileIndex >= _projectileIndicesToRandomize.Get.Length)
                        _forcedProjectileIndex = 0;

                    changedProjectileIndex = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_forcedProjectileIndex < 0)
                        _forcedProjectileIndex = _projectileIndicesToRandomize.Get.Length - 1;

                    changedProjectileIndex = true;
                }

                if (changedProjectileIndex)
                {
                    Log.Debug($"Current projectile override: {ProjectileCatalog.GetProjectilePrefab(_projectileIndicesToRandomize.Get[_forcedProjectileIndex]).name} ({_projectileIndicesToRandomize.Get[_forcedProjectileIndex]})");
                }
            }
        }
#endif
    }
}
