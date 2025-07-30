using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IPokemonService
{
    /// <summary>
    /// Returns a Pokemon matching the Pokemon id
    /// </summary>
    /// <param name="id">The Pokemon id</param>
    public Task<Pokemon> GetPokemonById(Guid id);

    /// <summary>
    /// Returns all Pokemon matching the trainer id for a certain game session
    /// </summary>
    /// <param name="trainerId">The trainer id</param>
    /// <param name="gameId">The game session id</param>
    public Task<IEnumerable<Pokemon>> GetPokemonByTrainerId(Guid trainerId, Guid gameId);

    /// <summary>
    /// Attempts to add a Pokemon using the provided document
    /// </summary>
    /// <param name="pokemon">The document to add</param>
    public Task PostPokemon(Pokemon pokemon);

    /// <summary>
    /// Attempts to replace the previous pokemon with the new data
    /// </summary>
    /// <param name="updatePokemon">The updated pokemon data</param>
    public Task<Pokemon> UpdatePokemon(Pokemon updatePokemon);

    /// <summary>
    /// Searches for a pokemon, then updates its trainer id
    /// </summary>
    /// <param name="pokemonId">The pokemon id</param>
    /// <param name="trainerId">The trainer id</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Pokemon> UpdatePokemonTrainerId(
        Guid pokemonId,
        Guid trainerId);

    /// <summary>
    /// Attempts to update a pokemon's hp
    /// </summary>
    /// <param name="pokemonId">The pokemon's id</param>
    /// <param name="hp">The pokemon's new hp</param>
    public Task<Pokemon> UpdatePokemonHP(Guid pokemonId, int hp);

    /// <summary>
    /// Searches for a pokemon, then updates its evolvability
    /// </summary>
    /// <param name="pokemonId">The pokemon id</param>
    /// <param name="isEvolvable">Whether the pokemon is evolvable</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Pokemon> UpdatePokemonEvolvability(
        Guid pokemonId,
        bool isEvolvable);

    /// <summary>
    /// Searches for a pokemon, then updates its location
    /// </summary>
    /// <param name="pokemonId">The pokemon id</param>
    /// <param name="isOnActiveTeam">Whether the pokemon is on the active team</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Pokemon> UpdatePokemonLocation(
        Guid pokemonId,
        bool isOnActiveTeam);

    /// <summary>
    /// Searches for a Pokemon using its id, then deletes it
    /// </summary>
    /// <param name="id">The Pokemon id</param>
    public Task DeletePokemon(Guid id);

    /// <summary>
    /// Searches for all Pokemon using their trainer id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    /// <param name="trainerId">The trainer id</param>
    public Task DeletePokemonByTrainerId(Guid gameId, Guid trainerId);
}
