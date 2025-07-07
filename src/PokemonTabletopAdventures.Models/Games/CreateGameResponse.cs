using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Games;

public class CreateGameResponse
{
    public required ICollection<Game> Games { get; set; }
}
