using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Users;

public class LoginRequest
{
    [MinLength(6), MaxLength(20)]
    public string Username { get; set; } = string.Empty;
    [MinLength(6), MaxLength(20)]
    public string Password { get; init; } = string.Empty;
    public Guid UserId { get; set; }
}
