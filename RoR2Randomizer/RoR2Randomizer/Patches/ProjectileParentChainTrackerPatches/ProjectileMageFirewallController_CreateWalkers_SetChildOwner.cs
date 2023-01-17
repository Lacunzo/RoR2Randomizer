using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;

namespace RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches
{
    [PatchClass]
    static class ProjectileMageFirewallController_CreateWalkers_SetChildOwner
    {
        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileMageFirewallController.CreateWalkers += ProjectileMageFirewallController_CreateWalkers;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileMageFirewallController.CreateWalkers -= ProjectileMageFirewallController_CreateWalkers;
        }

        static void ProjectileMageFirewallController_CreateWalkers(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int patchesMade = 0;

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdfld<ProjectileMageFirewallController>(nameof(ProjectileMageFirewallController.walkerPrefab)),
                                 x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<ProjectileManager>(_ => _.FireProjectile(default, default, default, default, default, default, default, default, default, default)))))
            {
                ILCursor ilCursor = foundCursors[1];
                ilCursor.Emit(OpCodes.Ldarg_0);
                ilCursor.EmitDelegate(static (ProjectileMageFirewallController instance) =>
                {
                    ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = instance.gameObject;
                });

                c.Index = foundCursors[foundCursors.Length - 1].Index + 1;

                patchesMade++;
            }

            if (patchesMade == 0)
            {
                Log.Warning("unable to find patch location");
            }
#if DEBUG
            else
            {
                Log.Debug($"patched {patchesMade} locations");
            }
#endif
        }
    }
}
