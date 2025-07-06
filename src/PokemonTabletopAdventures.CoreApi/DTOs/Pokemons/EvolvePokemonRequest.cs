using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class EvolvePokemonRequest
{
    [Required, StringLength(255, MinimumLength = 1)]
    public required string NextForm { get; set; }
    public required IEnumerable<string> KeptMoves { get; set; }
    public required IEnumerable<string> NewMoves { get; set; }
}
