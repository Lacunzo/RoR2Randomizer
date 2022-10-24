using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileExplosion_FireChild_SetChildOwner
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileExplosion.FireChild += ProjectileExplosion_FireChild;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileExplosion.FireChild -= ProjectileExplosion_FireChild;
        }

        static void ProjectileExplosion_FireChild(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ProjectileExplosion>(nameof(ProjectileExplosion.childrenProjectilePrefab)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => GameObject.Instantiate<GameObject>(default(GameObject), default(Vector3), default(Quaternion))))))
            {
                ILCursor ilCursor = foundCursors[1];
                ilCursor.Index++;
                ilCursor.Emit(OpCodes.Dup);
                ilCursor.Emit(OpCodes.Ldarg_0);
                ilCursor.EmitDelegate(static (GameObject childProjectile, ProjectileExplosion instance) =>
                {
                    if (childProjectile.TryGetComponent<ProjectileParentChainTracker>(out ProjectileParentChainTracker parentChainTracker))
                    {
                        parentChainTracker.TrySetParent(instance.gameObject);
                    }
                });
            }
        }
    }
}
