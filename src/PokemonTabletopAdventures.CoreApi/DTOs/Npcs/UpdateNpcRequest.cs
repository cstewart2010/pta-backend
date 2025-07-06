using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class UpdateNpcRequest
{
    [Required]
    public required IEnumerable<Npc> Npcs { get; set; }
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
}
