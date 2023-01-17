using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.CharacterLimiter;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.CharacterLimiting
{
    [PatchClass]
    static class Goobo
    {
        static void Apply()
        {
            IL.RoR2.Projectile.GummyCloneProjectile.SpawnGummyClone += GummyCloneProjectile_SpawnGummyClone;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.GummyCloneProjectile.SpawnGummyClone -= GummyCloneProjectile_SpawnGummyClone;
        }

        static void GummyCloneProjectile_SpawnGummyClone(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(static (DirectorSpawnRequest gooboSpawnRequest, GummyCloneProjectile instance) =>
                {
                    if (gooboSpawnRequest != null)
                    {
                        GameObject owner;
                        if (instance && instance.TryGetComponent<ProjectileController>(out ProjectileController projectileController) &&
                            projectileController.owner && projectileController.owner.TryGetComponent<CharacterBody>(out CharacterBody ownerBody))
                        {
                            owner = ownerBody.masterObject;
                        }
                        else
                        {
                            owner = null;
                        }

                        MiscUtils.AppendDelegate(ref gooboSpawnRequest.onSpawnedServer, (SpawnCard.SpawnResult result) =>
                        {
                            if (result.success && result.spawnedInstance)
                            {
                                LimitedCharacterData.AddGeneration(result.spawnedInstance, owner, LimitedCharacterType.Goobo);
                            }
                        });
                    }
                });
            }
            else
            {
                Log.Warning("unable to find patch location");
            }
        }
    }
}
