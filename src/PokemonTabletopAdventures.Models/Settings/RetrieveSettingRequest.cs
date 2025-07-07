using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Settings;

public class RetrieveSettingRequest
{
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
}