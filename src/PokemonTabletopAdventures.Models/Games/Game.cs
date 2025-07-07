using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.Models.Games;

public class Game
{
    public required string Nickname { get; set; }
    public required Guid GameId { get; set; }
    public required bool IsOnline { get; set; }
    public required IEnumerable<Trainer> Trainers { get; set; }
    public required IEnumerable<Npc> Npcs { get; set; }
    public required IEnumerable<Setting> Settings { get; set; }
    public required ICollection<Log> Logs { get; set; }
}
