using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Shops;

public class UpdateShopRequest
{
    [Required]
    public required Guid UserId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    public ICollection<Shop> Shops { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
}