using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Games;

public class DeleteGameRequest
{
    [Required]
    public Guid GameMasterId { get; set; }
    [Required]
    public string? GameSessionPassword { get; set; }
}
