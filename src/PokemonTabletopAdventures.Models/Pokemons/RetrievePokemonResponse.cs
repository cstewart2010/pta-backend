namespace PokemonTabletopAdventures.Models.Pokemons;

public class RetrievePokemonResponse
{
    public ICollection<Pokemon> Pokemon { get; set; } = [];
    public ICollection<PokemonForm> Models { get; set; } = [];
}
