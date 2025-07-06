using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Indicies;

public class IndexResponse<T> where T : IDexDocument
{
    public required T Data { get; set; }
}
