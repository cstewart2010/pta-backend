using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokedex
{
    public class UpdatePokedexResponse
    {
        public required ICollection<PokedexItem> PokedexItems { get; set; }
    }
}
