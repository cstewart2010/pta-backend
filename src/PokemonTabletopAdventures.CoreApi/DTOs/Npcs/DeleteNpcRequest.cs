using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs
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
