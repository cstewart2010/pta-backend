namespace PokemonTabletopAdventures.Models.Shops;

public class CreateShopResponse
{
    public required ICollection<Shop> Shops { get; set; }
}