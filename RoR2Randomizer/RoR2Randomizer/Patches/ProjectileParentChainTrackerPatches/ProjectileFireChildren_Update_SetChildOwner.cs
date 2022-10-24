using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileFireChildren_Update_SetChildOwner
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileFireChildren.Update += ProjectileFireChildren_Update;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileFireChildren.Update -= ProjectileFireChildren_Update;
        }

        static void ProjectileFireChildren_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ProjectileFireChildren>(nameof(ProjectileFireChildren.childProjectilePrefab)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => GameObject.Instantiate<GameObject>(default(GameObject), default(Vector3), default(Quaternion))))))
            {
                ILCursor ilCursor = foundCursors[1];
                ilCursor.Index++;
                ilCursor.Emit(OpCodes.Dup);
                ilCursor.Emit(OpCodes.Ldarg_0);
                ilCursor.EmitDelegate(static (GameObject childProjectile, ProjectileFireChildren instance) =>
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
