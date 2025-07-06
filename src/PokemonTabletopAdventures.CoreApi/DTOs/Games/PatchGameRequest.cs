using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class PatchGameRequest
{
    [Required]
    public required Guid UserId { get; set; }
    public Guid GameMasterId { get; set; }
    public string? GameSessionPassword { get; set; }
    [Required]
    public required Game Game { get; set; }
}
