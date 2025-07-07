namespace PokemonTabletopAdventures.Models.Trainers;

public class UpdateTrainerResponse
{
    public required ICollection<Trainer> Trainers { get; set; }
}