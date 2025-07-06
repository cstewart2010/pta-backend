using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Users;

public class PutLoginRequest
{
    [Required, MinLength(6), MaxLength(20)]
    public required string Username { get; set; }
    [Required, MinLength(6), MaxLength(20)]
    public required string Password { get; init; }
}
