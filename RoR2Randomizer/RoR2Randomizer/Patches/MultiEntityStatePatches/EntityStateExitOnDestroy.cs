using EntityStates;
using RoR2;
using RoR2Randomizer.CustomContent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.MultiEntityStatePatches
{
    [PatchClass]
    public static class EntityStateExitOnDestroy
    {
        static void Apply()
        {
            On.EntityStates.EntityState.Destroy += EntityState_Destroy;
        }

        static void Cleanup()
        {
            On.EntityStates.EntityState.Destroy -= EntityState_Destroy;
        }

        static void EntityState_Destroy(On.EntityStates.EntityState.orig_Destroy orig, UnityEngine.Object obj)
        {
            if (obj && obj is GameObject gameObject && gameObject.TryGetComponent<EntityStateMachine>(out EntityStateMachine stateMachine))
            {
                if (stateMachine.state is MultiEntityState multiState)
                {
                    if (!multiState.IsWaitingForForceExitDestruction)
                    {
                        IEnumerable<MethodBase> excludeMethodsBase = new StackTrace().GetFrames()
                            .Select(f => f.GetMethod())
                            .Where(m => m != null && m.IsVirtual && typeof(EntityState).IsAssignableFrom(m.DeclaringType));

                        // Prevent infinite recursion
                        IEnumerable<Type> excludeExit = excludeMethodsBase.Where(m => m.Name == nameof(EntityState.OnExit))
                                                                          .Select(m => m.DeclaringType);

                        IEnumerable<Type> excludeEnter = excludeMethodsBase.Where(m => m.Name == nameof(EntityState.OnEnter))
                                                                           .Select(m => m.DeclaringType);

                        multiState.forceExit(new HashSet<Type>(excludeExit), new HashSet<Type>(excludeEnter));
                    }

                    return;
                }
            }

            orig(obj);
        }
    }
}
