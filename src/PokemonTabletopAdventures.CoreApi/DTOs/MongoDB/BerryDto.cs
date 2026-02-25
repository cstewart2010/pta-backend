using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an berry in the BerryDex
/// </summary>
public class BerryDto : IDocument, IDexDocument
{
    /// <inheritdoc/>
    [JsonIgnore]
    public ObjectId Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The berry's cost
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// The effects of using the berry
    /// </summary>
    public string Effects { get; set; } = string.Empty;

    /// <summary>
    /// The berry's flavors
    /// </summary>
    public string Flavors { get; set; } = string.Empty;

    /// <summary>
    /// The berry's rarity
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public Rarity Rarity { get; set; }
}
