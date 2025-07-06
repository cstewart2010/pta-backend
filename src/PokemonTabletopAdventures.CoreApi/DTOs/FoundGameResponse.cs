using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class FoundGameResponse : AbstractDto
    {
        public required string Nickname { get; init; }
        public required Guid GameId { get; init; }
        public required IEnumerable<Trainer> Trainers { get; init; }
    }
}
