using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons
{
    public class DeletePokemonRequest
    {
        [Required]
        public required Guid GameMasterId { get; set; }
        [Required]
        public required Guid GameId { get; set; }
        [Required]
        public required Guid PokemonId { get; set; }
    }
}
