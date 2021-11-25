using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a Pokemon in Pokemon Tabletop Adventures
    /// </summary>
    public class PokemonModel: IDocument
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
        /// The species name for the pokemon
        /// </summary>
        public string SpeciesName { get; set; }

        /// <summary>
        /// The trainer's unique id
        /// </summary>
        public string TrainerId { get; set; }

        /// <summary>
        /// The Pokemon's gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The Pokemon's status
        /// </summary>
        public string PokemonStatus { get; set; }

        /// <summary>
        /// The Pokemon's nickname. Defaults to the Species name is nothing is selected
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The Pokemon's moves
        /// </summary>
        public IEnumerable<string> Moves { get; set; }

        /// <summary>
        /// The Pokemon's type positioning
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The Pokemon's catchrate
        /// </summary>
        public int CatchRate { get; set; }

        /// <summary>
        /// The Pokemon's nature positioning
        /// </summary>
        public string Nature { get; set; }

        /// <summary>
        /// Whether the pokemon is shiny or not
        /// </summary>
        public bool IsShiny { get; set; }

        /// <summary>
        /// Whether the pokemon is on the team
        /// </summary>
        public bool IsOnActiveTeam { get; set; }

        /// <summary>
        /// Collection of Pokemon stats
        /// </summary>
        public StatsModel PokemonStats { get; set; }

        public string Size { get; set; }

        public string Weight { get; set; }

        public IEnumerable<string> Skills { get; set; }

        public IEnumerable<string> Passives { get; set; }

        public IEnumerable<string> EggGroups { get; set; }

        public IEnumerable<string> Proficiencies { get; set; }

        public string EggHatchRate { get; set; }

        public IEnumerable<string> Habitats { get; set; }

        public string Diet { get; set; }

        public string Rarity { get; set; }

        public string GMaxMove { get; set; }

        public string EvolvedFrom { get; set; }

        public LegendaryStatsModel LegendaryStats { get; set; }
    }
}
