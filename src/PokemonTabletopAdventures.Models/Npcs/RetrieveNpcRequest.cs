using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Npcs;

public class RetrieveNpcRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid NpcId { get; set; }
}
