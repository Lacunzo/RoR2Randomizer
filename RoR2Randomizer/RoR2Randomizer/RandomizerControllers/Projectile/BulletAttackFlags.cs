using System;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    [Flags]
    public enum BulletAttackFlags : uint
    {
        None = 0,
        Sniper = 1 << 0
    }
}
