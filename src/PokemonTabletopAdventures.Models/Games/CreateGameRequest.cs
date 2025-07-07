using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Games;

public class CreateGameRequest
{
    [Required]
    public required Guid UserId { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string Username { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string GameSessionPassword { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string GameNickname { get; set; }
}
