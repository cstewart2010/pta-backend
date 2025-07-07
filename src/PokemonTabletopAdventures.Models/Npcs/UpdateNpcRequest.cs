using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Npcs;

public class UpdateNpcRequest
{
    [Required]
    public required IEnumerable<Npc> Npcs { get; set; }
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
}
