using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;

public class CapturePokemonRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid TrainerId { get; set; }
    [Required]
    public required WildPokemon Pokemon { get; set; }
}
