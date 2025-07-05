using MongoDB.Bson;
using PokemonTabletopAdventures.Interfaces;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents an berry in the BerryDex
/// </summary>
public class BerryModel : IDocument, IDexDocument
{
    /// <inheritdoc/>
    public ObjectId _id { get; set; }

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
    public Rarity Rarity { get; set; }
}
