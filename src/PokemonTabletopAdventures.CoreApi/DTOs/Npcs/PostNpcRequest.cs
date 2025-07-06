using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class PostNpcRequest
{
    public required string TrainerName { get; init; }
    public required IEnumerable<string> Feat { get; init; }
    public required IEnumerable<string> Classes { get; init; }
}
