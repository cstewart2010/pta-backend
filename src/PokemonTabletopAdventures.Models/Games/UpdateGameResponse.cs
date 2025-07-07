namespace PokemonTabletopAdventures.Models.Games;

public class UpdateGameResponse
{
    public required ICollection<Game> Games { get; set; }
}
