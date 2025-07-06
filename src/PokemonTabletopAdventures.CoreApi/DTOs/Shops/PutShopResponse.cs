using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.Models;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Shops;

public class PutShopResponse
{
    public required Trainer Trainer { get; set; }
    public required ShopModel Shop { get; set; }
}
