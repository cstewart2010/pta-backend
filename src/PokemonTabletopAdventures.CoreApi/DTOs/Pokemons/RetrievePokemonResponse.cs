using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons
{
    public class RetrievePokemonResponse
    {
        public required ICollection<Pokemon> Pokemons { get; set; }
    }
}
