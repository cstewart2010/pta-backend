using System;

namespace TheReplacements.PTA.Common.Attributes
{
    internal class NatureModifierAttribute : Attribute
    {
        public int HpModifier { get; }
        public int AttackModifier { get; }
        public int DefenseModifier { get; }
        public int SpecialAttackModifier { get; }
        public int SpecialDefenseModifier { get; }
        public int SpeedModifier { get; }

        public NatureModifierAttribute(
            int hpModifier = 0,
            int attackModifier = 0,
            int defenseModifier = 0,
            int specialAttackModifier = 0,
            int specialDefenseModifier = 0,
            int speedModifier = 0)
        {
            HpModifier = hpModifier;
            AttackModifier = attackModifier;
            DefenseModifier = defenseModifier;
            SpecialAttackModifier = specialAttackModifier;
            SpecialDefenseModifier = specialDefenseModifier;
            SpeedModifier = speedModifier;
        }
    }
}
