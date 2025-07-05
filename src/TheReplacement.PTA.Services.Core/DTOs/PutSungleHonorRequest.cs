using System;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class PutSungleHonorRequest
    {
        public required string Honor { get; set; }
        public required Guid TrainerId { get; set; } 
    }
}
