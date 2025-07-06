using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class DeleteGameRequest
{
    [Required]
    public Guid GameMasterId { get; set; }
    [Required]
    public string? GameSessionPassword { get; set; }
}
