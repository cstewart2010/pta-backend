using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class UpdatePokemonRequest
{
    public Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid TrainerId { get; set; }
    [Required]
    public required Guid PokemonId { get; set; }
    [Range(-1000, 1000)]
    public int HP { get; set; }
    public string? Form { get; set; }
    public EvolvePokemonData? EvolvePokemonData { get; set; }
}
