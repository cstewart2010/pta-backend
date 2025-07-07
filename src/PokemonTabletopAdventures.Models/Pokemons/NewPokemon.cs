namespace PokemonTabletopAdventures.Models.Pokemons;

public class NewPokemon
{
    public required string SpeciesName { get; set; }
    public required string Nickname { get; set; }
    public required bool IsOnActiveTeam { get; set; }
    public required string Form { get; set; }
}
