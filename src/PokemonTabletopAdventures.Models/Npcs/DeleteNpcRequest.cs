using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Npcs
{
    public class DeleteNpcRequest
    {
        [Required]
        public required Guid GameMasterId { get; set; }
        [Required]
        public required Guid GameId { get; set; }
        public Guid NpcId { get; set; }
    }
}
