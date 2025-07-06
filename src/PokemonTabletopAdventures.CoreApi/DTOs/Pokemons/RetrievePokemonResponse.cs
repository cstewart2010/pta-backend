using PokemonTabletopAdventures.Models;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class RetrievePokemonResponse
{
    public ICollection<Pokemon> Pokemon { get; set; } = [];
    public ICollection<BasePokemonModel> Models { get; set; } = [];
}
