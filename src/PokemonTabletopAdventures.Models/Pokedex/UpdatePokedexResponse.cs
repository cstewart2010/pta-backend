namespace PokemonTabletopAdventures.Models.Pokedex;

public class UpdatePokedexResponse
{
    public required ICollection<PokedexItem> PokedexItems { get; set; }
}
