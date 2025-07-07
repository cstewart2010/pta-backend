using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Trainers;

public class CreateTrainerRequest
{
    [Required]
    public required Trainer Trainer { get; set; }
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid UserId { get; set; }
}