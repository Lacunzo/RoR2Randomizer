using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Reflection;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class ProjectileFireChildren_ReplaceChildPrefab
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileFireChildren.Update += ProjectileFireChildren_Update_IL;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileFireChildren.Update -= ProjectileFireChildren_Update_IL;
        }

        static GameObject _originalPrefab;

        static readonly FieldInfo _prefab_FI = AccessTools.DeclaredField(typeof(ProjectileFireChildren_ReplaceChildPrefab), nameof(_prefab));
        static GameObject _prefab;

        static readonly FieldInfo _position_FI = AccessTools.DeclaredField(typeof(ProjectileFireChildren_ReplaceChildPrefab), nameof(_position));
        static Vector3 _position;

        static readonly FieldInfo _rotation_FI = AccessTools.DeclaredField(typeof(ProjectileFireChildren_ReplaceChildPrefab), nameof(_rotation));
        static Quaternion _rotation;

        static void ProjectileFireChildren_Update_IL(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(ProjectileFireChildren_ReplaceChildPrefab)}.{nameof(ProjectileFireChildren_Update_IL)} (patch) ";

            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ProjectileFireChildren>(nameof(ProjectileFireChildren.nextSpawnTimer)),
                              x => x.MatchLdfld<ProjectileFireChildren>(nameof(ProjectileFireChildren.duration)),
                              x => x.MatchLdfld<ProjectileFireChildren>(nameof(ProjectileFireChildren.count)),
                              x => x.MatchBlt(out _) || x.MatchBltUn(out _)))
            {
                c = foundCursors[3];
                if (c.Next.Operand is ILLabel label)
                {
                    if (c.TryGotoNext(x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => GameObject.Instantiate<GameObject>(default, default(Vector3), default(Quaternion))))))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate(static (GameObject prefab, Vector3 position, Quaternion rotation, ProjectileFireChildren instance) =>
                        {
                            ProjectileController controller = instance.projectileController;
                            ProjectileDamage damage = instance.projectileDamage;

                            ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = instance.gameObject;

                            if (ProjectileRandomizerController.TryReplaceProjectileInstantiateFire(ref prefab, out _originalPrefab, position, rotation, controller.owner, damage.damage, damage.force, damage.crit, damage.damageType))
                            {
                                _prefab = prefab;
                                _position = position;
                                _rotation = rotation;

                                return true;
                            }

                            return false;
                        });

                        c.Emit(OpCodes.Brfalse, label);

                        c.Emit(OpCodes.Ldsfld, _prefab_FI);
                        c.Emit(OpCodes.Ldsfld, _position_FI);
                        c.Emit(OpCodes.Ldsfld, _rotation_FI);
                    }
                    else
                    {
                        Log.Warning(LOG_PREFIX + "failed (1)");
                    }

                    ILCursor ret = c.Goto(label.Target);
                    ret.Emit(OpCodes.Ldarg_0);
                    ret.EmitDelegate(static (ProjectileFireChildren instance) =>
                    {
                        if (_originalPrefab)
                        {
                            instance.childProjectilePrefab = _originalPrefab;
                        }
                    });
                }
                else
                {
                    Log.Warning(LOG_PREFIX + "failed (1)");
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + "failed (0)");
            }
        }
    }
}
