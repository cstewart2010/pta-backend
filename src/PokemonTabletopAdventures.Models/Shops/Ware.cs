using PokemonTabletopAdventures.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Shops;

public class Ware
{
    /// <summary>
    /// The item's cost
    /// </summary>
    [Required]
    public required int Cost { get; set; }

    /// <summary>
    /// The effects of using the item
    /// </summary>
    [Required]
    public required string Effects { get; set; } = string.Empty;

    /// <summary>
    /// The type of item it is
    /// </summary>
    [Required]
    public required StartingEquipmentType Type { get; set; }

    /// <summary>
    /// The amount of item on sale
    /// </summary>
    [Required]
    public required int Quantity { get; set; }
}
