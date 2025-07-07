namespace PokemonTabletopAdventures.Models.Indicies;

public class IndexCollectionResponse
{
    public required int Count { get; set; }
    public required IEnumerable<string> Results { get; set; }
}
