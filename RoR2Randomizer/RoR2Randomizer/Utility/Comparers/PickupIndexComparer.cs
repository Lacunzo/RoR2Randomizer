using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility.Comparers
{
    public sealed class PickupIndexComparer : IComparer<PickupIndex>
    {
        public static readonly PickupIndexComparer Instance = new PickupIndexComparer();

        PickupIndexComparer()
        {
        }

        public int Compare(PickupIndex x, PickupIndex y)
        {
            return x.value.CompareTo(y.value);
        }
    }
}
