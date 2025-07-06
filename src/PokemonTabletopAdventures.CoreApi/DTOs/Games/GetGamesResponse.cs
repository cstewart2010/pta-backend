using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class GetGamesResponse
{
    public required ICollection<Game> Games { get; set; }
}

