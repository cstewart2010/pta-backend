using System;
using TheReplacement.PTA.Common.Enums;

namespace TheReplacement.PTA.Common.Attributes
{
    internal class NatureModifierAttribute : Attribute
    {
        public NatureModifierAttribute(
            Flavors likedFlavor,
            Flavors dislikedFlavor,
            int attackModifier = 0,
            int defenseModifier = 0,
            int specialAttackModifier = 0,
            int specialDefenseModifier = 0,
            int speedModifier = 0)
        {
            LikedFlavor = likedFlavor;
            DislikedFlavor = dislikedFlavor;
            AttackModifier = attackModifier;
            DefenseModifier = defenseModifier;
            SpecialAttackModifier = specialAttackModifier;
            SpecialDefenseModifier = specialDefenseModifier;
            SpeedModifier = speedModifier;
        }

        public Flavors LikedFlavor { get; }

        public Flavors DislikedFlavor { get; }

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
