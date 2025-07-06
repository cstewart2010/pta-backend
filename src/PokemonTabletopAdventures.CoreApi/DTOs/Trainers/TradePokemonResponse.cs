using PokemonTabletopAdventures.Models;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Trainers;

public class TradePokemonResponse
{
    public required PokemonModel LeftPokemon { get; set; }
    public required PokemonModel RightPokemon { get; set; }
}
