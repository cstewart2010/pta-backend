using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Shops;

public class RetrieveShopRequest
{
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public Guid SettingId { get; set; }
}