using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Games;

public class UpdateGameRequest
{
    [Required]
    public required Guid UserId { get; set; }
    public Guid GameMasterId { get; set; }
    [MinLength(6), MaxLength(20)]
    public string? GameSessionPassword { get; set; }
    [Required]
    public required Game Game { get; set; }
}
