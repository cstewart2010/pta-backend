using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class TradePokemonRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid LeftPokemonId { get; set; }
    [Required]
    public required Guid RightPokemonId { get; set; }
}
