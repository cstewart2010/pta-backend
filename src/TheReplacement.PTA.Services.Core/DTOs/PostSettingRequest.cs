using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class PostSettingRequest
    {
        public required string Name { get; set; }
        public required SettingType Type { get; set; }
    }
}
