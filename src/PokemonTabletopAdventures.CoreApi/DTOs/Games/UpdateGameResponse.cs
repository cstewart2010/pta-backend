using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class UpdateGameResponse
{
    public required ICollection<Game> Games { get; set; }
}
