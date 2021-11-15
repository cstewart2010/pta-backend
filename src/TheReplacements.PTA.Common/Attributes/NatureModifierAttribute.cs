using System;

namespace TheReplacements.PTA.Common.Attributes
{
    internal class NatureModifierAttribute : Attribute
    {
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

        /// <summary>
        /// Returns the HP nature modifier
        /// </summary>
        public int HpModifier { get; }

        /// <summary>
        /// Returns the Attack nature modifier
        /// </summary>
        public int AttackModifier { get; }

        /// <summary>
        /// Returns the Defense nature modifier
        /// </summary>
        public int DefenseModifier { get; }

        /// <summary>
        /// Returns the SpecialAttack nature modifier
        /// </summary>
        public int SpecialAttackModifier { get; }

        /// <summary>
        /// Returns the SpecialDefense nature modifier
        /// </summary>
        public int SpecialDefenseModifier { get; }

        /// <summary>
        /// Returns the Speed nature modifier
        /// </summary>
        public int SpeedModifier { get; }
    }
}
