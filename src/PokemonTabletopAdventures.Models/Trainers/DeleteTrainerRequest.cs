using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Trainers;

public class DeleteTrainerRequest
{
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid GameMasterId { get; set; }
    [Required]
    public required Guid TrainerId { get; set; }
}