using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Trainers;

public class UpdateTrainerRequest
{
    [Required]
    public required Guid GameId { get; set; }
    [Required]
    public required Guid UserId { get; set; }
    [Required]
    public required ICollection<Trainer> Trainers { get; set; }
    public string? Honor { get; set; }
}