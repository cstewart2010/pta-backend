using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class CreatePokemonResponse
{
    public required ICollection<Pokemon> Pokemon { get; set; }
}
