using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Indicies;

public class IndexCollectionResponse
{
    internal IndexCollectionResponse(
        int count,
        IEnumerable<string> results)
    {
        Count = count;
        Results = results;
    }

    public int Count { get; }
    public IEnumerable<string> Results { get; }
}
