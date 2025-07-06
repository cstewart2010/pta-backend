using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class RetrieveNpcRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    public required Guid NpcId { get; set; }
}
