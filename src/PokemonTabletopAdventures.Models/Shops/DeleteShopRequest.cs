using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Shops;

public class DeleteShopRequest
{
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid UserId { get; set; }
    [Required]
    public Guid ShopId { get; set; }
}