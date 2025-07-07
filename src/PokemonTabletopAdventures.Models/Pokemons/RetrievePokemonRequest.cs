using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Pokemons;

public class RetrievePokemonRequest
{
    public Guid GameMasterId { get; set; }
    public Guid GameId { get; set; }
    public Guid TrainerId { get; set; }
    [Required]
    public required Guid PokemonId { get; set; }
}
