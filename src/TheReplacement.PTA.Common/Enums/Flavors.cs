using System;

namespace TheReplacement.PTA.Common.Enums
{
    [Flags]
    public enum Flavors
    {
        Spicy = 1,
        Sour = 2,
        Dry = 4,
        Bitter = 8,
        Sweet = 16
    }
}
