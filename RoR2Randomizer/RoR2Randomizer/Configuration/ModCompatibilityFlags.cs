using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    [Flags]
    public enum ModCompatibilityFlags : byte
    {
        None = 0,
        RiskOfOptions = 1 << 0
    }
}
