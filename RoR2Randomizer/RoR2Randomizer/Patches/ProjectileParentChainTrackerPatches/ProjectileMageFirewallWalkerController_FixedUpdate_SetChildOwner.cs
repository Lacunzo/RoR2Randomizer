using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileMageFirewallWalkerController_FixedUpdate_SetChildOwner
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileMageFirewallWalkerController.FixedUpdate += ProjectileMageFirewallWalkerController_FixedUpdate;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileMageFirewallWalkerController.FixedUpdate -= ProjectileMageFirewallWalkerController_FixedUpdate;
        }

        static void ProjectileMageFirewallWalkerController_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdfld<ProjectileMageFirewallController>(nameof(ProjectileMageFirewallController.walkerPrefab)),
                                 x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<ProjectileManager>(_ => _.FireProjectile(default, default, default, default, default, default, default, default, default, default)))))
            {
                ILCursor ilCursor = foundCursors[1];
                ilCursor.Emit(OpCodes.Ldarg_0);
                ilCursor.EmitDelegate(static (ProjectileMageFirewallWalkerController instance) =>
                {
                    ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = instance.gameObject;
                });

                c.Index = foundCursors[foundCursors.Length - 1].Index + 1;
            }
        }
    }
}
