using System;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class CreatePokemonRequest
{
    public Guid GameMasterId { get; set; }
    public required Guid GameId { get; set; }
    public required Guid TrainerId { get; set; }
    public required IEnumerable<NewPokemon> Pokemon { get; set; }
}
