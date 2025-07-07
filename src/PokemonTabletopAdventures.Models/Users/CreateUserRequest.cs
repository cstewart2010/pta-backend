using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Users;

public class CreateuUserRequest
{
    [Required, MinLength(6), MaxLength(20)]
    public required string Username { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string Password { get; init; }
}