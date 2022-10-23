using System;

namespace RoR2Randomizer.Networking
{
    [Flags]
    public enum MessageProviderFlags : byte
    {
        None = 0,
        Persistent = 1 << 0
    }
}
