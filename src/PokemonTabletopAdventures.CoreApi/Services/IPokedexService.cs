using PokemonTabletopAdventures.Models.Pokedex;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IPokedexService
{
    /// <summary>
    /// Compiles all pokedex entries for a specific trainer into one collection
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    public Task<ICollection<PokedexItem>> GetTrainerPokeDex(Guid trainerId, Guid gameId);

    /// <summary>
    /// Searches the database for a pokedex entry
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="dexNo">The dex number for the pokemon</param>
    public Task<PokedexItem> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo);

    /// <summary>
    /// Attempts to add a dexItem using the provided document
    /// </summary>
    /// <param name="trainerId">The pokedex's trainer id</param>
    /// <param name="gameId">The pokedex's game id</param>
    /// <param name="dexNo">The dex number</param>
    /// <param name="isSeen">Whether the pokemon was seen</param>
    /// <param name="isCaught">Whether the pokemon was caught</param>
    public Task PostDexItem(
        Guid trainerId,
        Guid gameId,
        int dexNo,
        bool isSeen,
        bool isCaught);

    /// <summary>
    /// Updates the pokedex entry for a seen pokemon
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="dexNo">The dex number for the pokemon</param>
    public Task<PokedexItem> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo);

    /// <summary>
    /// Updates the pokedex entry for a caught pokemon
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="dexNo">The dex number for the pokemon</param>
    public Task<PokedexItem> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo);

    public Task DeleteTrainerDex(Guid trainerId, Guid gameId);
}
