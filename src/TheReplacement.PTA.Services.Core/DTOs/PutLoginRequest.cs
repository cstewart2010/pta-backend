using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class PutLoginRequest
    {
        [Required, StringLength(20, MinimumLength = 6)]
        public required string Username { get; set; }
        [Required, StringLength(20, MinimumLength = 6)]
        public required string Password { get; init; }
    }
}
