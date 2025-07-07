using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Settings;

public class UpdateSettingRequest
{
    [Required]
    public required Guid TrainerId { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Setting Setting { get; set; }
}