using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.Models.Shops;

public class UpdateShopResponse
{
    public ICollection<Shop> Shops { get; set; } = [];
    public Trainer? Trainer { get; set; }
}