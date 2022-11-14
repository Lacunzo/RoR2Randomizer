using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.Orbs.LightningOrbHandling
{
    public struct LightningOrbIdentifier
    {
        public static readonly LightningOrbIdentifier Invalid = new LightningOrbIdentifier { Index = -1 };

        public int Index = -1;

        public readonly LightningOrb.LightningType LightningType;
        public readonly uint DefaultNumBounces;
        public readonly uint TargetsToFindPerBounce;

        public readonly bool IsValid => Index >= 0;

        public LightningOrbIdentifier(LightningOrb.LightningType lightningType, uint defaultNumBounces, uint targetsToFindPerBounce)
        {
            LightningType = lightningType;
            DefaultNumBounces = defaultNumBounces;
            TargetsToFindPerBounce = targetsToFindPerBounce;
        }

        public LightningOrbIdentifier(LightningOrb lightningOrb) : this(lightningOrb.lightningType, 0U, 1U)
        {
        }

        public LightningOrbIdentifier(NetworkReader reader)
        {
            Index = reader.ReadPackedIndex32();
            LightningType = (LightningOrb.LightningType)reader.ReadPackedUInt32();
            DefaultNumBounces = reader.ReadPackedUInt32();
            TargetsToFindPerBounce = reader.ReadPackedUInt32();
        }

        public readonly void Serialize(NetworkWriter writer)
        {
            writer.WritePackedIndex32(Index);
            writer.WritePackedUInt32((uint)LightningType);
            writer.WritePackedUInt32(DefaultNumBounces);
            writer.WritePackedUInt32(TargetsToFindPerBounce);
        }

        public readonly bool Matches(LightningOrb lightningOrb)
        {
            return LightningType == lightningOrb.lightningType;
        }

        public readonly bool Equals(LightningOrbIdentifier other, bool compareIndex)
        {
            if (compareIndex)
            {
                return Index == other.Index;
            }
            else
            {
                return LightningType == other.LightningType;
            }
        }

        public override readonly bool Equals(object obj)
        {
            return obj is LightningOrbIdentifier other && Equals(other, true);
        }

        public static bool operator ==(LightningOrbIdentifier a, LightningOrbIdentifier b)
        {
            return a.Equals(b, true);
        }

        public static bool operator !=(LightningOrbIdentifier a, LightningOrbIdentifier b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hashCode = 1383804432;
            hashCode = (hashCode * -1521134295) + Index.GetHashCode();
            hashCode = (hashCode * -1521134295) + LightningType.GetHashCode();
            return hashCode;
        }

        public readonly LightningOrb CreateInstance()
        {
            return new LightningOrb
            {
                lightningType = LightningType,
                bouncesRemaining = (int)DefaultNumBounces,
                bouncedObjects = new List<HealthComponent>(),
                targetsToFindPerBounce = (int)TargetsToFindPerBounce
            };
        }

        public override readonly string ToString()
        {
            return $"{LightningType} ({Index})";
        }

        public static implicit operator ProjectileTypeIdentifier(LightningOrbIdentifier lightningOrb)
        {
            return new ProjectileTypeIdentifier(ProjectileType.LightningOrb, lightningOrb.Index);
        }
    }
}
