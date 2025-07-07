namespace PokemonTabletopAdventures.Models.Pokemons;

public class CreatePokemonResponse
{
    public required ICollection<Pokemon> Pokemon { get; set; }
}
