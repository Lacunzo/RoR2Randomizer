using System;

namespace RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling
{
    [Flags]
    public enum BulletAttackFlags : uint
    {
        None = 0,
        Sniper = 1 << 0
    }
}
