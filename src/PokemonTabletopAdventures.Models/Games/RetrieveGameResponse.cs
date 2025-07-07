namespace PokemonTabletopAdventures.Models.Games;

public class RetrieveGameResponse
{
    public required ICollection<Game> Games { get; set; }
}

