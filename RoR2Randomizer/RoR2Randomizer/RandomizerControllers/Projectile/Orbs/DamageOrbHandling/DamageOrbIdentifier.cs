using HG;
using RoR2;
using RoR2.Orbs;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling
{
    public struct DamageOrbIdentifier : ICatalogIdentifier<GenericDamageOrb, DamageOrbIdentifier>, IEquatable<DamageOrbIdentifier>
    {
        public static readonly DamageOrbIdentifier Invalid = new DamageOrbIdentifier() { Index = -1 };

        public int Index { readonly get; set; }

        public readonly EffectIndex OrbEffectIndex;
        public readonly SerializableSystemType OrbType;

        public readonly bool IsValid => IndexValid && ValuesValid;

        public readonly bool IndexValid => Index != -1;
        public readonly bool ValuesValid => OrbEffectIndex != EffectIndex.Invalid && (Type)OrbType != null;

        public DamageOrbIdentifier(EffectIndex orbEffectIndex, SerializableSystemType orbType)
        {
            OrbEffectIndex = orbEffectIndex;
            OrbType = orbType;
        }

        public DamageOrbIdentifier(GenericDamageOrb damageOrb) : this(EffectCatalog.FindEffectIndexFromPrefab(damageOrb.GetOrbEffect()), (SerializableSystemType)damageOrb.GetType())
        {
        }

        public DamageOrbIdentifier(NetworkReader reader)
        {
            Index = reader.ReadPackedIndex32();
            OrbEffectIndex = reader.ReadEffectIndex();
            OrbType = reader.ReadSerializableType();
        }

        public readonly void Serialize(NetworkWriter writer)
        {
            writer.WritePackedIndex32(Index);
            writer.WriteEffectIndex(OrbEffectIndex);
            writer.WriteSerializableType(OrbType);
        }

        public readonly bool Equals(in DamageOrbIdentifier other, bool compareIndex)
        {
            if (compareIndex)
            {
                return Index == other.Index;
            }
            else
            {
                return OrbEffectIndex == other.OrbEffectIndex && OrbType == other.OrbType;
            }
        }

        public readonly bool Equals(DamageOrbIdentifier other)
        {
            return Equals(other, true);
        }

        public override readonly string ToString()
        {
            return $"{nameof(Index)}={Index} {nameof(OrbEffectIndex)}={OrbEffectIndex} {nameof(OrbType)}={((Type)OrbType)?.Name ?? "null"}";
        }

        public readonly bool Matches(GenericDamageOrb damageOrb)
        {
            return OrbEffectIndex == EffectCatalog.FindEffectIndexFromPrefab(damageOrb.GetOrbEffect());
        }

        public GenericDamageOrb CreateInstance()
        {
            GenericDamageOrb instance = (GenericDamageOrb)Activator.CreateInstance((Type)OrbType);
            return instance;
        }

        public static implicit operator ProjectileTypeIdentifier(in DamageOrbIdentifier damageOrbIdentifier)
        {
            return new ProjectileTypeIdentifier(ProjectileType.DamageOrb, damageOrbIdentifier.Index);
        }
    }
}
