using PokemonTabletopAdventures.Models;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class PutShopResponse
    {
        public required Trainer Trainer { get; set; }
        public required ShopModel Shop { get; set; }
    }
}
