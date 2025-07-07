using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class CreatePokemonRequest
{
    public Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid TrainerId { get; set; }
    [Required]
    public required IEnumerable<NewPokemon> Pokemon { get; set; }
}
