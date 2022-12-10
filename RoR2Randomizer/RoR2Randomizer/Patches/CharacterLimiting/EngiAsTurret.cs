using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.CharacterLimiter;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.CharacterLimiting
{
    [PatchClass]
    static class EngiAsTurret
    {
        static MasterCatalog.MasterIndex _engiMasterIndex;

        [SystemInitializer(typeof(MasterCatalog))]
        static void Init()
        {
            _engiMasterIndex = MasterCatalog.FindMasterIndex("EngiMonsterMaster");
        }

        static void Apply()
        {
            IL.RoR2.CharacterBody.HandleConstructTurret += CharacterBody_HandleConstructTurret;
        }

        static void Cleanup()
        {
            IL.RoR2.CharacterBody.HandleConstructTurret -= CharacterBody_HandleConstructTurret;
        }

        static void CharacterBody_HandleConstructTurret(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(CharacterLimiting)}.{nameof(EngiAsTurret)}.{nameof(CharacterBody_HandleConstructTurret)} ";

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<MasterSummon>(nameof(MasterSummon.Perform))))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(static (CharacterMaster summoned, CharacterBody owner) =>
                {
                    if (NetworkServer.active && _engiMasterIndex.isValid)
                    {
                        if (summoned && summoned.masterIndex == _engiMasterIndex)
                        {
                            GameObject ownerObj;
                            if (owner && owner.master)
                            {
                                ownerObj = owner.master.gameObject;

                                if (owner.master.masterIndex != _engiMasterIndex)
                                    return;
                            }
                            else
                            {
                                ownerObj = null;
                            }

                            LimitedCharacterData.AddGeneration(summoned.gameObject, ownerObj, LimitedCharacterType.EngiAsTurret);
                        }
                    }
                });
            }
            else
            {
                Log.Warning(LOG_PREFIX + "unable to find patch location");
            }
        }
    }
}
