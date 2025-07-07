using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB.Interfaces;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a Pokemon in Pokemon Tabletop Adventures
/// </summary>
internal class PokemonDto: IDocument
{
    /// <inheritdoc />
    public ObjectId _id { get; set; }

    /// <summary>
    /// The Pokemon's unique id
    /// </summary>
    public Guid PokemonId { get; set; }

    /// <summary>
    /// The Pokemon's dex number
    /// </summary>
    public int DexNo { get; set; }

    /// <summary>
    /// The Pokemon species' form name
    /// </summary>
    public string Form { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon species' alternate forms, if any
    /// </summary>
    public IEnumerable<string> AlternateForms { get; set; } = [];

    /// <summary>
    /// The Pokemon species' normal image
    /// </summary>
    public string NormalPortrait { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon species' shiny image
    /// </summary>
    public string ShinyPortrait { get; set; } = string.Empty;

    /// <summary>
    /// The species name for the pokemon
    /// </summary>
    public string SpeciesName { get; set; } = string.Empty;

    /// <summary>
    /// The trainer's unique id
    /// </summary>
    public Guid TrainerId { get; set; }

    /// <summary>
    /// The game's unique id
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// The pokemon's original trainer id
    /// </summary>
    public Guid OriginalTrainerId { get; set; }

    /// <summary>
    /// The Pokemon's gender
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Gender Gender { get; set; }

    /// <summary>
    /// The Pokemon's status
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Status PokemonStatus { get; set; }

    /// <summary>
    /// The Pokemon's nickname. Defaults to the Species name is nothing is selected
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's moves
    /// </summary>
    public IEnumerable<string> Moves { get; set; } = [];

    /// <summary>
    /// The Pokemon's type positioning
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's catchrate
    /// </summary>
    public int CatchRate { get; set; }

    /// <summary>
    /// The Pokemon's nature positioning
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Nature Nature { get; set; }

    /// <summary>
    /// Whether the pokemon is shiny or not
    /// </summary>
    public bool IsShiny { get; set; }

    /// <summary>
    /// Whether the pokemon is on the team
    /// </summary>
    public bool IsOnActiveTeam { get; set; }

    /// <summary>
    /// Whether the pokemon is ready to evolve
    /// </summary>
    public bool CanEvolve { get; set; }

    /// <summary>
    /// Collection of Pokemon stats
    /// </summary>
    public Stats PokemonStats { get; set; } = new();

    /// <summary>
    /// The pokemon's current pokeball
    /// </summary>
    public string Pokeball { get; set; } = string.Empty;

    /// <summary>
    /// The pokemon'ss current hp
    /// </summary>
    public int CurrentHP { get; set; }

    /// <summary>
    /// The Pokemon species' <see cref="Enums.Size"/>
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Size Size { get; set; }

    /// <summary>
    /// The Pokemon species' <see cref="Enums.Weight"/>
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Weight Weight { get; set; }

    /// <summary>
    /// The Pokemon species' Skills
    /// </summary>
    public IEnumerable<string> Skills { get; set; } = [];

    /// <summary>
    /// The Pokemon's Passives
    /// </summary>
    public IEnumerable<string> Passives { get; set; } = [];

    /// <summary>
    /// The Pokemon's <see cref="Enums.EggGroups"/>
    /// </summary>
    public IEnumerable<string> EggGroups { get; set; } = [];

    /// <summary>
    /// The Pokemon's Proficiencies
    /// </summary>
    public IEnumerable<string> Proficiencies { get; set; } = [];

    /// <summary>
    /// The Pokemon's hatch rate
    /// </summary>
    public string EggHatchRate { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's habitats
    /// </summary>
    public IEnumerable<string> Habitats { get; set; } = [];

    /// <summary>
    /// The Pokemon's diet
    /// </summary>
    public string Diet { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's <see cref="Enums.Rarity"/>
    /// </summary>
    public string Rarity { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's GMax move, if any
    /// </summary>
    public string GMaxMove { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's previous evolution, if any
    /// </summary>
    public string EvolvedFrom { get; set; } = string.Empty;

    /// <summary>
    /// The Pokemon's legendary stats, if applicable
    /// </summary>
    public LegendaryStats? LegendaryStats { get; set; }
}
