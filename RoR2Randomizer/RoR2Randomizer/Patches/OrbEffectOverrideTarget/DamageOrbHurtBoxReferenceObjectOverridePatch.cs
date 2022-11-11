using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2.Orbs;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile.DamageOrbHandling;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.OrbEffectOverrideTarget
{
    [PatchClass]
    static class DamageOrbHurtBoxReferenceObjectOverridePatch
    {
        internal static readonly Dictionary<GenericDamageOrb, Vector3> overrideOrbTargetPosition = new Dictionary<GenericDamageOrb, Vector3>();

        static readonly Hook Orb_get_distanceToTarget = new Hook(AccessTools.DeclaredPropertyGetter(typeof(Orb), nameof(Orb.distanceToTarget)), static (Func<Orb, float> orig, Orb self) =>
        {
            float result = orig(self);
            if (!self.target && self is GenericDamageOrb genericDamageOrb && overrideOrbTargetPosition.TryGetValue(genericDamageOrb, out Vector3 overrideTargetPosition))
            {
                result = Vector3.Distance(self.origin, overrideTargetPosition);
            }

            return result;
        }, new HookConfig { ManualApply = true });

        static void Apply()
        {
            IL.RoR2.Orbs.GenericDamageOrb.Begin += IL_GenericDamageOrb_Begin;

            Orb_get_distanceToTarget.Apply();
        }

        static void Cleanup()
        {
            IL.RoR2.Orbs.GenericDamageOrb.Begin -= IL_GenericDamageOrb_Begin;

            Orb_get_distanceToTarget.Undo();
        }

        static readonly FieldInfo _target_FI = AccessTools.DeclaredField(typeof(DamageOrbHurtBoxReferenceObjectOverridePatch), nameof(_target));
#pragma warning disable CS0649 // Compiler complains that it is never assigned since it's only set through patched IL
        static HurtBox _target;
#pragma warning restore CS0649

        static void IL_GenericDamageOrb_Begin(ILContext il)
        {
            const string LOG_PREFIX = $"Patch {nameof(DamageOrbHurtBoxReferenceObjectOverridePatch)}.{nameof(IL_GenericDamageOrb_Begin)} ";

            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchNewobj<EffectData>(),
                              x => x.MatchStloc(out _)))
            {
                if (foundCursors[1].Next.MatchStloc(out int effectDataLocalIndex))
                {
                    if (c.TryGotoNext(x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<EffectData>(_ => _.SetHurtBoxReference(default(HurtBox))))))
                    {
                        c.Emit(OpCodes.Dup); // dup target
                        c.Emit(OpCodes.Stsfld, _target_FI);
                        c.Index++;
                        c.Emit(OpCodes.Ldloc, effectDataLocalIndex);
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate(static (EffectData effectData, GenericDamageOrb instance) =>
                        {
                            if (!_target && ProjectileRandomizerController.IsActive && overrideOrbTargetPosition.TryGetValue(instance, out Vector3 overrideTargetPosition))
                            {
                                DamageOrbTargetDummyObjectMarker marker = DamageOrbTargetDummyObjectMarker.GetMarker(overrideTargetPosition, instance.duration * 2f);
                                if (marker)
                                {
                                    effectData.SetHurtBoxReference(marker.gameObject);
                                }
                            }
                        });
                    }
                    else
                    {
                        Log.Warning(LOG_PREFIX + $"failed to find {nameof(EffectData.SetHurtBoxReference)} call");
                    }
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"failed to find {nameof(EffectData)} local index");
            }

            if (c.TryGotoNext(x => x.MatchRet()))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(static (GenericDamageOrb instance) =>
                {
                    overrideOrbTargetPosition.Remove(instance);
                });
            }
            else
            {
                Log.Warning(LOG_PREFIX + "failed to end of method");
            }
        }
    }
}
