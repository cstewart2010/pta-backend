using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class PostGameRequest
{
    [Required]
    public required Guid UserId { get; set; }
    [Required, StringLength(20, MinimumLength = 6)]
    public required string Username { get; set; }
    [Required, StringLength(20, MinimumLength = 6)]
    public required string GameSessionPassword { get; set; }
    [Required, StringLength(20, MinimumLength = 6)]
    public required string GameNickname { get; set; }
}
