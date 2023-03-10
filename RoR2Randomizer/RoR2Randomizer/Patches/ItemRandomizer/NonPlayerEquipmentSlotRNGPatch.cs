using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class NonPlayerEquipmentSlotRNGPatch
    {
        static void Apply()
        {
            IL.RoR2.EquipmentSlot.Start += EquipmentSlot_Start;
        }

        static void Cleanup()
        {
            IL.RoR2.EquipmentSlot.Start -= EquipmentSlot_Start;
        }

        static void EquipmentSlot_Start(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchNewobj<Xoroshiro128Plus>()))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((ulong seed, EquipmentSlot instance) =>
                {
                    CharacterBody ownerBody = instance.characterBody;
                    if (ownerBody)
                    {
                        if (!ownerBody.isPlayerControlled)
                        {
                            seed ^= instance.netId.Value;
                        }
                    }

                    return seed;
                });
            }
            else
            {
                Log.Error("Failed to find patch location");
            }
        }
    }
}
