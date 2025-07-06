using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents an item sold at a shop
/// </summary>
public class WareModel
{
    /// <summary>
    /// The item's cost
    /// </summary>
    public int Cost { get; set; }

    /// <summary>
    /// The effects of using the item
    /// </summary>
    public string Effects { get; set; } = string.Empty;

    /// <summary>
    /// The type of item it is
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public StartingEquipmentType Type { get; set; }

    /// <summary>
    /// The amount of item on sale
    /// </summary>
    public int Quantity { get; set; }
}
