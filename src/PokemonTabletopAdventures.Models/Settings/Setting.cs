using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.Models.Settings;

public class Setting
{
    public required Guid SettingId { get; set; }
    public required Guid GameId { get; set; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
    public required SettingType Type { get; set; }
    public required string[] Environment { get; set; }
    public required IEnumerable<Shop> Shops { get; set; }
    public required ICollection<SettingParticipant> Participants { get; set; }
}
