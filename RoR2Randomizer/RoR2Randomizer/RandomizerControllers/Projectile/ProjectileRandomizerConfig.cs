using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    public sealed class ProjectileRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeHitscanAttacks;

        public readonly BoolConfigValue ExcludeInstakillProjeciles;

#if DEBUG
        public ProjectileTypeIdentifier DebugProjectileIdentifier => EnableDebugProjectile ? new ProjectileTypeIdentifier(DebugProjectileType.Parsed, DebugProjectileIndex.Parsed) : ProjectileTypeIdentifier.Invalid;

        public readonly BoolConfigValue EnableDebugProjectile;
        public readonly EnumStringConfigValue<ProjectileType> DebugProjectileType;
        public readonly IntStringConfigValue DebugProjectileIndex;
#endif

        public ProjectileRandomizerConfig(ConfigFile file) : base("Projectile", file)
        {
            RandomizeHitscanAttacks = new BoolConfigValue(getEntry("Randomize Hitscan Attacks", "If Hitscan attacks can be randomized into projectiles, and vice verse.", true));

            ExcludeInstakillProjeciles = new BoolConfigValue(getEntry("Exclude Instakill Projectiles", "Excludes instakill projectiles from the projectile randomizer", false));

#if DEBUG
            EnableDebugProjectile = new BoolConfigValue(getEntry("Enable Debug Projectile", false));
            DebugProjectileType = new EnumStringConfigValue<ProjectileType>(getEntry("Debug Projectile Type", "Invalid"), ProjectileType.Invalid, true);
            DebugProjectileIndex = new IntStringConfigValue(getEntry("Debug Projectile Index", "-1"), -1);
#endif
        }
    }
}
