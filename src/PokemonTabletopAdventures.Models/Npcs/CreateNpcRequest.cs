using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Npcs;

public class CreateNpcRequest
{
    [Required]
    public required ICollection<Npc> Npcs { get; set; }
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
}
