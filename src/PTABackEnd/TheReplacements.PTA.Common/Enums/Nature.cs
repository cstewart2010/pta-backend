using TheReplacements.PTA.Common.Attributes;

namespace TheReplacements.PTA.Common.Enums
{
    internal enum Nature
    {
        None,
        [NatureModifier(hpModifier: 2, attackModifier: -2)]
        Hardy,
        [NatureModifier(hpModifier: 2, defenseModifier: -2)]
        Docile,
        [NatureModifier(hpModifier: 2, specialAttackModifier: -2)]
        Proud,
        [NatureModifier(hpModifier: 2, specialDefenseModifier: -2)]
        Quirky,
        [NatureModifier(hpModifier: 2, speedModifier: -2)]
        Lazy,
        [NatureModifier(attackModifier: 2, hpModifier: -1)]
        Desperate,
        [NatureModifier(attackModifier: 2, defenseModifier: -2)]
        Lonely,
        [NatureModifier(attackModifier: 2, specialAttackModifier: -2)]
        Adamant,
        [NatureModifier(attackModifier: 2, specialDefenseModifier: -2)]
        Naughty,
        [NatureModifier(attackModifier: 2, speedModifier: -2)]
        Brave,
        [NatureModifier(defenseModifier: 2, hpModifier: -1)]
        Stark,
        [NatureModifier(defenseModifier: 2, attackModifier: -2)]
        Bold,
        [NatureModifier(defenseModifier: 2, specialAttackModifier: -2)]
        Impish,
        [NatureModifier(defenseModifier: 2, specialDefenseModifier: -2)]
        Lax,
        [NatureModifier(defenseModifier: 2, speedModifier: -2)]
        Relaxed,
        [NatureModifier(specialAttackModifier: 2, hpModifier: -1)]
        Bashful,
        [NatureModifier(specialAttackModifier: 2, attackModifier: -2)]
        Modest,
        [NatureModifier(specialAttackModifier: 2, defenseModifier: -2)]
        Mild,
        [NatureModifier(specialAttackModifier: 2, specialDefenseModifier: -2)]
        Rash,
        [NatureModifier(specialAttackModifier: 2, speedModifier: -2)]
        Quiet,
        [NatureModifier(specialDefenseModifier: 2, hpModifier: -1)]
        Sickly,
        [NatureModifier(specialDefenseModifier: 2, attackModifier: -2)]
        Calm,
        [NatureModifier(specialDefenseModifier: 2, defenseModifier: -2)]
        Gently,
        [NatureModifier(specialDefenseModifier: 2, specialAttackModifier: -2)]
        Careful,
        [NatureModifier(specialDefenseModifier: 2, speedModifier: -2)]
        Sassy,
        [NatureModifier(speedModifier: 2, hpModifier: -1)]
        Serious,
        [NatureModifier(speedModifier: 2, attackModifier: -2)]
        Timid,
        [NatureModifier(speedModifier: 2, defenseModifier: -2)]
        Hasty,
        [NatureModifier(speedModifier: 2, specialAttackModifier: -2)]
        Jolly,
        [NatureModifier(speedModifier: 2, specialDefenseModifier: -2)]
        Naive,
        [NatureModifier()]
        Composed,
        [NatureModifier()]
        Dull,
        [NatureModifier()]
        Patient,
        [NatureModifier()]
        Poised,
        [NatureModifier()]
        Stoic
    }
}
