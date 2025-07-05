using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class WildPokemon
{
    public required string Pokemon { get; set; }
    public required string Nature { get; set; }
    public required string Gender { get; set; }
    public required string Status { get; set; }
    [Required, StringLength(255, MinimumLength = 1)]
    public required string Form { get; set; }
    public required bool ForceShiny { get; set; } 
}
