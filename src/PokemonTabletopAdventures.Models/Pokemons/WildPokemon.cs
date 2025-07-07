using PokemonTabletopAdventures.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class WildPokemon
{
    [Required, MinLength(1), MaxLength(255)]
    public required string Pokemon { get; set; }
    [Required]
    public required Nature Nature { get; set; }
    [Required]
    public required Gender Gender { get; set; }
    [Required]
    public required Status Status { get; set; }
    [Required, MinLength(1), MaxLength(255)]
    public required string Form { get; set; }
    [Required]
    public required bool ForceShiny { get; set; } 
}
