using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface INpcService
{
    /// <summary>
    /// Returns an npc matching the npc id
    /// </summary>
    /// <param name="id">The npc id</param>
    public Task<NpcModel> GetNpc(Guid id);

    /// <summary>
    /// Returns all npcs matching the npc ids
    /// </summary>
    /// <param name="npcIds">The npc ids</param>
    public Task<IEnumerable<NpcModel>> GetNpcs(IEnumerable<Guid> npcIds);

    /// <summary>
    /// Returns all npcs matching the game id
    /// </summary>
    /// <param name="gameId">The npc ids</param>
    public Task<IEnumerable<NpcModel>> GetNpcsByGameId(Guid gameId);

    /// <summary>
    /// Attempts to add an npc using the provided document
    /// </summary>
    /// <param name="npc">The document to add</param>
    public Task PostNpc(NpcModel npc);

    /// <summary>
    /// Attempts to replace the previous Npc with the new data
    /// </summary>
    /// <param name="updatedNpc">The updated npc data</param>
    public Task<NpcModel> UpdateNpc(NpcModel updatedNpc);

    /// <summary>
    /// Searches for an npc using its id, then deletes it
    /// </summary>
    /// <param name="id">The npc id</param>
    public Task DeleteNpc(Guid id);

    /// <summary>
    /// Searches for all Npcs using their game id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task DeleteNpcByGameId(Guid gameId);
}
