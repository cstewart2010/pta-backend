namespace PokemonTabletopAdventures.Models.Trainers;

public class RetrieveTrainerResponse
{
    public required ICollection<Trainer> Trainers { get; set; }
}