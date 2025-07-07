using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface INpcService
{
    /// <summary>
    /// Returns an npc matching the npc id
    /// </summary>
    /// <param name="id">The npc id</param>
    public Task<Npc> GetNpc(Guid id);

    /// <summary>
    /// Returns all npcs matching the npc ids
    /// </summary>
    /// <param name="npcIds">The npc ids</param>
    public Task<IEnumerable<Npc>> GetNpcs(IEnumerable<Guid> npcIds);

    /// <summary>
    /// Returns all npcs matching the game id
    /// </summary>
    /// <param name="gameId">The npc ids</param>
    public Task<IEnumerable<Npc>> GetNpcsByGameId(Guid gameId);

    /// <summary>
    /// Attempts to add an npc using the provided document
    /// </summary>
    /// <param name="npc">The document to add</param>
    public Task PostNpc(Npc npc);

    /// <summary>
    /// Attempts to replace the previous Npc with the new data
    /// </summary>
    /// <param name="updatedNpc">The updated npc data</param>
    public Task<Npc> UpdateNpc(Npc updatedNpc);

    /// <summary>
    /// Searches for an npc using its id, then deletes it
    /// </summary>
    /// <param name="id">The npc id</param>
    public Task DeleteNpc(Guid id, IGameService gameService);

    /// <summary>
    /// Searches for all Npcs using their game id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task DeleteNpcByGameId(Guid gameId, IGameService gameService);
}
