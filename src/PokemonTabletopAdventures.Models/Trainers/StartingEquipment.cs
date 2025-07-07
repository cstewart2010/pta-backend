using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models.Trainers;

/// <summary>
/// Represents a Starting Equipment Item
/// </summary>
public class StartingEquipment
{
    /// <summary>
    /// The name of the item
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of item
    /// </summary>
    public required StartingEquipmentType Type { get; set; }

    /// <summary>
    /// The Amount of item to start with.
    /// </summary>
    public required int Amount { get; set; }
}