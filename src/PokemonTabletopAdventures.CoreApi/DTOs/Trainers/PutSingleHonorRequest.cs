using System;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Trainers;

public class PutSingleHonorRequest
{
    public required string Honor { get; set; }
    public required Guid TrainerId { get; set; } 
}
