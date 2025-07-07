using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Trainers;

public class RetrieveTrainerRequest
{
    [Required]
    public required Guid GameId { get; set; }
    public Guid TrainerId { get; set; }
    public string? TrainerName { get; set; }
}