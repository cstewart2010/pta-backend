using TheReplacement.PTA.Common.Attributes;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Represents a container for valid Pokemon natures
    /// </summary>
    public enum Nature
    {
        /// <summary>
        /// Represents no valid nature
        /// </summary>
        None,

        /// <summary>
        /// Represents a Hardy nature (HP: 2, Attack: -2)
        /// </summary>
        [NatureModifier(hpModifier: 2, attackModifier: -2)]
        Hardy,

        /// <summary>
        /// Represents a Docile nature (HP: 2, Defense: -2)
        /// </summary>
        [NatureModifier(hpModifier: 2, defenseModifier: -2)]
        Docile,

        /// <summary>
        /// Represents a Proud nature (HP: 2, SpecialAttack: -2)
        /// </summary>
        [NatureModifier(hpModifier: 2, specialAttackModifier: -2)]
        Proud,

        /// <summary>
        /// Represents a Quirky nature (HP: 2, SpecialDefense: -2)
        /// </summary>
        [NatureModifier(hpModifier: 2, specialDefenseModifier: -2)]
        Quirky,

        /// <summary>
        /// Represents a Lazy nature (HP: 2, Speed: -2)
        /// </summary>
        [NatureModifier(hpModifier: 2, speedModifier: -2)]
        Lazy,

        /// <summary>
        /// Represents a Desperate nature (Attack: 2, HP: -1)
        /// </summary>
        [NatureModifier(attackModifier: 2, hpModifier: -1)]
        Desperate,

        /// <summary>
        /// Represents a Lonely nature (Attack: 2, Defense: -2)
        /// </summary>
        [NatureModifier(attackModifier: 2, defenseModifier: -2)]
        Lonely,

        /// <summary>
        /// Represents a Adamant nature (Attack: 2, SpecialAttack: -2)
        /// </summary>
        [NatureModifier(attackModifier: 2, specialAttackModifier: -2)]
        Adamant,

        /// <summary>
        /// Represents a Naughty nature (Attack: 2, SpecialDefense: -2)
        /// </summary>
        [NatureModifier(attackModifier: 2, specialDefenseModifier: -2)]
        Naughty,

        /// <summary>
        /// Represents a Brave nature (Attack: 2, Speed: -2)
        /// </summary>
        [NatureModifier(attackModifier: 2, speedModifier: -2)]
        Brave,

        /// <summary>
        /// Represents a Stark nature (Defense: 2, HP: -1)
        /// </summary>
        [NatureModifier(defenseModifier: 2, hpModifier: -1)]
        Stark,

        /// <summary>
        /// Represents a Bold nature (Defense: 2, Attack: -2)
        /// </summary>
        [NatureModifier(defenseModifier: 2, attackModifier: -2)]
        Bold,

        /// <summary>
        /// Represents a Impish nature (Defense: 2, SpecialAttack: -2)
        /// </summary>
        [NatureModifier(defenseModifier: 2, specialAttackModifier: -2)]
        Impish,

        /// <summary>
        /// Represents a Lax nature (Defense: 2, SpecialDefense: -2)
        /// </summary>
        [NatureModifier(defenseModifier: 2, specialDefenseModifier: -2)]
        Lax,

        /// <summary>
        /// Represents a Relaxed nature (Defense: 2, Speed: -2)
        /// </summary>
        [NatureModifier(defenseModifier: 2, speedModifier: -2)]
        Relaxed,

        /// <summary>
        /// Represents a Bashful nature (SpecialAttack: 2, HP: -1)
        /// </summary>
        [NatureModifier(specialAttackModifier: 2, hpModifier: -1)]
        Bashful,

        /// <summary>
        /// Represents a Modest nature (SpecialAttack: 2, Attack: -2)
        /// </summary>
        [NatureModifier(specialAttackModifier: 2, attackModifier: -2)]
        Modest,

        /// <summary>
        /// Represents a Mild nature (SpecialAttack: 2, Defense: -2)
        /// </summary>
        [NatureModifier(specialAttackModifier: 2, defenseModifier: -2)]
        Mild,

        /// <summary>
        /// Represents a Rash nature (SpecialAttack: 2, SpecialDefense: -2)
        /// </summary>
        [NatureModifier(specialAttackModifier: 2, specialDefenseModifier: -2)]
        Rash,

        /// <summary>
        /// Represents a Quiet nature (SpecialAttack: 2, Speed: -2)
        /// </summary>
        [NatureModifier(specialAttackModifier: 2, speedModifier: -2)]
        Quiet,

        /// <summary>
        /// Represents a Sickly nature (SpecialDefense: 2, HP: -1)
        /// </summary>
        [NatureModifier(specialDefenseModifier: 2, hpModifier: -1)]
        Sickly,

        /// <summary>
        /// Represents a Calm nature (SpecialDefense: 2, Attack: -2)
        /// </summary>
        [NatureModifier(specialDefenseModifier: 2, attackModifier: -2)]
        Calm,

        /// <summary>
        /// Represents a Gently nature (SpecialDefense: 2, Defense: -2)
        /// </summary>
        [NatureModifier(specialDefenseModifier: 2, defenseModifier: -2)]
        Gently,

        /// <summary>
        /// Represents a Careful nature (SpecialDefense: 2, SpecialAttack: -2)
        /// </summary>
        [NatureModifier(specialDefenseModifier: 2, specialAttackModifier: -2)]
        Careful,

        /// <summary>
        /// Represents a Sassy nature (SpecialDefense: 2, Speed: -2)
        /// </summary>
        [NatureModifier(specialDefenseModifier: 2, speedModifier: -2)]
        Sassy,

        /// <summary>
        /// Represents a Serious nature (Speed: 2, HP: -1)
        /// </summary>
        [NatureModifier(speedModifier: 2, hpModifier: -1)]
        Serious,

        /// <summary>
        /// Represents a Timid nature (Speed: 2, Attack: -2)
        /// </summary>
        [NatureModifier(speedModifier: 2, attackModifier: -2)]
        Timid,

        /// <summary>
        /// Represents a Hasty nature (Speed: 2, Defense: -2)
        /// </summary>
        [NatureModifier(speedModifier: 2, defenseModifier: -2)]
        Hasty,

        /// <summary>
        /// Represents a Jolly nature (Speed: 2, SpecialAttack: -2)
        /// </summary>
        [NatureModifier(speedModifier: 2, specialAttackModifier: -2)]
        Jolly,

        /// <summary>
        /// Represents a Naive nature (Speed: 2, SpecialDefense: -2)
        /// </summary>
        [NatureModifier(speedModifier: 2, specialDefenseModifier: -2)]
        Naive,

        /// <summary>
        /// Represents a Composed nature (No Stat changes)
        /// </summary>
        [NatureModifier]
        Composed,

        /// <summary>
        /// Represents a Dull nature (No Stat changes)
        /// </summary>
        [NatureModifier]
        Dull,

        /// <summary>
        /// Represents a Patient nature (No Stat changes)
        /// </summary>
        [NatureModifier]
        Patient,

        /// <summary>
        /// Represents a Poised nature (No Stat changes)
        /// </summary>
        [NatureModifier]
        Poised,

        /// <summary>
        /// Represents a Stoic nature (No Stat changes)
        /// </summary>
        [NatureModifier]
        Stoic
    }
}
