using System.Text.Json.Serialization;
using MongoDB.Bson;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an origin in the OriginDex
/// </summary>
public class OriginDto : IDocument, IDexDocument
{
    /// <inheritdoc/>
    [JsonIgnore]
    public ObjectId Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The Skill granted by the Origin
    /// </summary>
    public string Skill { get; set; } = string.Empty;

    /// <summary>
    /// The lifestyle granted by the Origin
    /// </summary>
    public string Lifestyle { get; set; } = string.Empty;

    /// <summary>
    /// The trainer's starting funds
    /// </summary>
    public int Savings { get; set; }

    /// <summary>
    /// The trainer's starting equipment
    /// </summary>
    public string Equipment { get; set; } = string.Empty;

    /// <summary>
    /// The trainer's starting equipment
    /// </summary>
    public IEnumerable<StartingEquipment> StartingEquipmentList { get; set; } = [];

    /// <summary>
    /// The trainer's starting pokemon, if applicable
    /// </summary>
    public string StartingPokemon { get; set; } = string.Empty;

    /// <summary>
    /// The Origin's specialized feature
    /// </summary>
    public FeatureDto Feature { get; set; } = new();
}

