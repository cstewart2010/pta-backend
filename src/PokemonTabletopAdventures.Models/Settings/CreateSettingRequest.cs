using PokemonTabletopAdventures.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Settings;

public class CreateSettingRequest
{
    [MinLength(6), MaxLength(20)]
    public required string Name { get; set; }
    [Required]
    public required SettingType Type { get; set; }
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
}
