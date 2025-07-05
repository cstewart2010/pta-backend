using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IPokedexService
{
    /// <summary>
    /// Compiles all pokedex entries for a specific trainer into one collection
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    public Task<IEnumerable<PokeDexItemModel>> GetTrainerPokeDex(Guid trainerId, Guid gameId);

    /// <summary>
    /// Searches the database for a pokedex entry
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="dexNo">The dex number for the pokemon</param>
    public Task<PokeDexItemModel> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo);

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
    public Task<PokeDexItemModel> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo);

    /// <summary>
    /// Updates the pokedex entry for a caught pokemon
    /// </summary>
    /// <param name="trainerId">The trainer's id to search with</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="dexNo">The dex number for the pokemon</param>
    public Task<PokeDexItemModel> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo);

    public Task DeleteDexItemForTrainer(Guid trainerId, Guid gameId);
}
