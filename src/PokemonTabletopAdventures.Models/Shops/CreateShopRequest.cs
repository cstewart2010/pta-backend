using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Shops;

public class CreateShopRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required, MinLength(1)]
    public required ICollection<Shop> Shops { get; set; }
}