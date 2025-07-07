namespace PokemonTabletopAdventures.Models.Shops;

public class Shop
{
    /// <summary>
    /// The shop's id
    /// </summary>
    public Guid ShopId { get; set; }

    /// <summary>
    /// The game's id
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// The shop's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the shop is active for trainers to visit
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// A collection of items on sale
    /// </summary>
    public Dictionary<string, Ware> Inventory { get; set; } = [];
}
