using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class RetrieveGameResponse
{
    public required ICollection<Game> Games { get; set; }
}

