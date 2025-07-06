using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Trainers;

public class PutSingleHonorRequest
{
    [MinLength(6), MaxLength(20)]
    public required string Honor { get; set; }
    public required Guid TrainerId { get; set; } 
}
