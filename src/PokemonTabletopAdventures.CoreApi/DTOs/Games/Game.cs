using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.DTOs.Settings;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using System;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class Game
{
    public required string Nickname { get; set; }
    public required Guid GameId { get; set; }
    public required IEnumerable<Trainer> Trainers { get; set; }
    public required IEnumerable<Npc> Npcs { get; set; }
    public required IEnumerable<Setting> Settings { get; set; }
    public required IEnumerable<Log> Logs { get; set; }
}
