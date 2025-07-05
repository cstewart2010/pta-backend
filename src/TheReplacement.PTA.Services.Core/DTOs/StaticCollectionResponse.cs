using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class StaticCollectionResponse<T>
{
    internal StaticCollectionResponse(
        int count,
        IEnumerable<T> results)
    {
        Count = count;
        Results = results;
    }

    public int Count { get; }
    public IEnumerable<T> Results { get; }
}
