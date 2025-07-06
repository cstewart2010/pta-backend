using PokemonTabletopAdventures.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Settings;

public class PostSettingRequest
{
    [MinLength(6), MaxLength(20)]
    public required string Name { get; set; }
    public required SettingType Type { get; set; }
}
