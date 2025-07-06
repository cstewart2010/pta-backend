using System;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games
{
    public class Log
    {
        public required string User { get; set; }
        public required string Action { get; set; }

        public required DateTimeOffset LogTimestamp { get; set; }
    }
}
