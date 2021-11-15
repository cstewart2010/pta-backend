using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    /// <summary>
    /// Represents a Pokemon in Pokemon Tabletop Adventures
    /// </summary>
    public class PokemonModel: IMongoCollectionModel
    {
        /// <inheritdoc />
        public ObjectId _id { get; set; }

        /// <summary>
        /// The Pokemon's unique id
        /// </summary>
        public string PokemonId { get; set; }

        /// <summary>
        /// The Pokemon's dex number
        /// </summary>
        public int DexNo { get; set; }

        /// <summary>
        /// The trainer's unique id
        /// </summary>
        public string TrainerId { get; set; }

        /// <summary>
        /// The Pokemon's gender
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// The Pokemon's nickname. Defaults to the Species name is nothing is selected
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The Pokemon's ability positioning
        /// </summary>
        public int Ability { get; set; }

        /// <summary>
        /// The Pokemon's naturally learned moves
        /// </summary>
        public IEnumerable<string> NaturalMoves { get; set; }

        /// <summary>
        /// The Pokemon's taught moves
        /// </summary>
        public IEnumerable<string> TMMoves { get; set; }

        /// <summary>
        /// The Pokemon's type positioning
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The Pokemon's total earn experience points
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// The Pokemon's current level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The Pokemon's exp yield
        /// </summary>
        public int ExpYield { get; set; }

        /// <summary>
        /// The Pokemon's catchrate
        /// </summary>
        public int CatchRate { get; set; }

        /// <summary>
        /// The Pokemon's nature positioning
        /// </summary>
        public int Nature { get; set; }

        /// <summary>
        /// Whether the pokemon is shiny or not
        /// </summary>
        public bool IsShiny { get; set; }

        /// <summary>
        /// Whether the pokemon is on the team
        /// </summary>
        public bool IsOnActiveTeam { get; set; }

        /// <summary>
        /// The Pokemon's HP Stat
        /// </summary>
        public PokemonStatModel HP { get; set; }

        /// <summary>
        /// The Pokemon's Attack Stat
        /// </summary>
        public PokemonStatModel Attack { get; set; }

        /// <summary>
        /// The Pokemon's Defense Stat
        /// </summary>
        public PokemonStatModel Defense { get; set; }

        /// <summary>
        /// The Pokemon's SpecialAttack Stat
        /// </summary>
        public PokemonStatModel SpecialAttack { get; set; }

        /// <summary>
        /// The Pokemon's SpecialDefense Stat
        /// </summary>
        public PokemonStatModel SpecialDefense { get; set; }

        /// <summary>
        /// The Pokemon's Speed Stat
        /// </summary>
        public PokemonStatModel Speed { get; set; }

        /// <summary>
        /// Aggregates all of the pokemon's raw stats to their respective totals
        /// </summary>
        public void AggregateStats()
        {
            HP.AggregateStatValue();
            Attack.AggregateStatValue();
            Defense.AggregateStatValue();
            SpecialAttack.AggregateStatValue();
            SpecialDefense.AggregateStatValue();
            Speed.AggregateStatValue();
        }
    }
}
