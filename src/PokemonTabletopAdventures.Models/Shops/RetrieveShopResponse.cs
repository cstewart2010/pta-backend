namespace PokemonTabletopAdventures.Models.Shops;

public class RetrieveShopResponse
{
    public required ICollection<Shop> Shops { get; set; }
}