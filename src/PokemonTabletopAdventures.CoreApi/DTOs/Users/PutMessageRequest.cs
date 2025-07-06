using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Users;

public class PutMessageRequest
{
    [Required, StringLength(255, MinimumLength = 1)]
    public required string MessageContent { get; set; }
}
