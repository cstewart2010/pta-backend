using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Container for all possible <see cref="SettingParticipantModelv1"/> types
    /// </summary>
    public enum SettingParticipantType
    {
        /// <summary>
        /// Represents a <see cref="TrainerModelv1"/>
        /// </summary>
        Trainer = 1,

        /// <summary>
        /// Represents a <see cref="PokemonModelv1"/>
        /// </summary>
        Pokemon = 2,

        /// <summary>
        /// Represents an Enemy <see cref="NpcModelv1"/>
        /// </summary>
        EnemyNpc = 3,

        /// <summary>
        /// Represents a Enemy <see cref="PokemonModelv1"/>
        /// </summary>
        EnemyPokemon = 4,

        /// <summary>
        /// Represents an Neutral <see cref="NpcModelv1"/>
        /// </summary>
        NeutralNpc = 5,

        /// <summary>
        /// Represents a Neutral <see cref="PokemonModelv1"/>
        /// </summary>
        NeutralPokemon = 6,

        /// <summary>
        /// Represents a Shop
        /// </summary>
        Shop = 7
    }
}
