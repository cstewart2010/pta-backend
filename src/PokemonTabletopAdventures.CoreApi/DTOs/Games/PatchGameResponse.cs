using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class PatchGameResponse
{
    public required ICollection<Game> Games { get; set; }
}
