using PokemonTabletopAdventures.CoreApi.Services;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class PostGameResponse
{
    public required ICollection<Game> Games { get; set; }
}
