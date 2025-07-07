namespace PokemonTabletopAdventures.Models.Games;

public class RetrieveLogsResponse
{
    public required ICollection<IEnumerable<Log>> LogPages { get; set; } = [];
}
