using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an move in the MoveDex
/// </summary>
public class MoveDto : IDocument, IDexDocument
{
    /// <inheritdoc/>
    [JsonIgnore]
    public ObjectId Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The range of the move
    /// </summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>
    /// The move's <see cref="PokemonTypes"/>
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public PokemonTypes Type { get; set; }

    /// <summary>
    /// The pokemon's stats
    /// </summary>
    public string Stat { get; set; } = string.Empty;

    /// <summary>
    /// The frequency at which the move can be used
    /// </summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// The damage rolls, if applicable
    /// </summary>
    public string DiceRoll { get; set; } = string.Empty;

    /// <summary>
    /// The effects of using the move
    /// </summary>
    public string Effects { get; set; } = string.Empty;

    /// <summary>
    /// The skills that the mvoe grants, if any
    /// </summary>
    public IEnumerable<string> GrantedSkills { get; set; } = [];

    /// <summary>
    /// The move's Contest stat, if any
    /// </summary>
    public string ContestStat { get; set; } = string.Empty;

    /// <summary>
    /// The move's Contest keyword, if any
    /// </summary>
    public string ContestKeyword { get; set; } = string.Empty;
}
