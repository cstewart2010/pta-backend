using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokedex
{
    public class UpdatePokedexRequest
    {
        public Guid GameMasterId { get; set; }
        public Guid GameId { get; set; }
        public Guid TrainerId { get; set; }
        [Required, MinLength(1)]
        public required ICollection<PokedexItem> PokedexItems { get; set; }
    }
}
