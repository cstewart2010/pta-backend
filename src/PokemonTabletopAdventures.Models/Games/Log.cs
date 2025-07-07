using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Games
{
    public class Log
    {
        [Required]
        public required string User { get; set; }
        [Required]
        public required string Action { get; set; }
        [Required]
        public required DateTimeOffset LogTimestamp { get; set; }
    }
}
