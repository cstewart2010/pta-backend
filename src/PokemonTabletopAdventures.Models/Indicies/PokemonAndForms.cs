using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.Models.Indicies;

public class PokemonAndForms
{
    public required PokemonForm Pokemon { get; init; }
    public required ICollection<string> AlternateForms { get; init; }
}
