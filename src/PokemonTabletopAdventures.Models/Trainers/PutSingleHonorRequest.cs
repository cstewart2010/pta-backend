using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Trainers;

public class PutSingleHonorRequest
{
    [MinLength(6), MaxLength(20)]
    public required string Honor { get; set; }
    public required Guid TrainerId { get; set; } 
}
