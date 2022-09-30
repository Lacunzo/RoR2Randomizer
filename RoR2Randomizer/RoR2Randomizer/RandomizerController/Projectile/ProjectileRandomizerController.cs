using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Projectile
{
    public class ProjectileRandomizerController : Singleton<ProjectileRandomizerController>
    {
        static readonly InitializeOnAccess<int[]> _projectileIndicesToRandomize = new InitializeOnAccess<int[]>(() =>
        {
            return ProjectileCatalog.projectilePrefabProjectileControllerComponents
                                    .Where(projectile =>
                                    {
                                        if (!projectile)
                                            return false;

                                        if (projectile.TryGetComponent<ProjectileFireChildren>(out ProjectileFireChildren projectileFireChildren)
                                            && !projectileFireChildren.childProjectilePrefab)
                                        {
#if DEBUG
                                            Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to invalid {nameof(ProjectileFireChildren)} setup");
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

        public static void TryOverrideProjectilePrefab(ref GameObject prefab)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue)
            {
                int originalIndex = ProjectileCatalog.GetProjectileIndex(prefab);
                if (_projectileIndicesReplacements.Value.TryGetReplacement(originalIndex, out int replacementIndex))
                {
                    GameObject replacementPrefab = ProjectileCatalog.GetProjectilePrefab(replacementIndex);
                    if (replacementPrefab)
                    {
#if DEBUG
                        Log.Debug($"Projectile randomizer: Replaced projectile: {prefab} ({originalIndex}) -> {replacementPrefab} ({replacementIndex})");
#endif

                        prefab = replacementPrefab;
                    }
                }
            }
        }

        public static bool TryGetOriginalProjectile(int replacementIndex, out int originalIndex)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue && _projectileIndicesReplacements.Value.TryGetOriginal(replacementIndex, out originalIndex))
            {
                return true;
            }

            originalIndex = -1;
            return false;
        }
    }
}
