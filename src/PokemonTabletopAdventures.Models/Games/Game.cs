using System.ComponentModel.DataAnnotations;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.Models.Games;

public class Game
{
    [Required]
    public required string Nickname { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required bool IsOnline { get; set; }
    [Required]
    public required IEnumerable<Trainer> Trainers { get; set; }
    [Required]
    public required IEnumerable<Npc> Npcs { get; set; }
    [Required]
    public required IEnumerable<Setting> Settings { get; set; }
    [Required]
    public required ICollection<Log> Logs { get; set; }
}
