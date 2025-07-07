using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class EvolvePokemonData
{
    [Required, MinLength(1), MaxLength(255)]
    public required string NextForm { get; set; }
    public required IEnumerable<string> KeptMoves { get; set; }
    public required IEnumerable<string> NewMoves { get; set; }
}
