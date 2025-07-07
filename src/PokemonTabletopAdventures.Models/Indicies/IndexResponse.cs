using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.Models.Indicies;

public class IndexResponse<T> where T : IDexDocument
{
    public required T Data { get; set; }
}
