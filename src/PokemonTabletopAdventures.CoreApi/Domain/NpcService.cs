using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class NpcService(IGameService gameService) : AbstractService<NpcModel>(MongoCollection.NPCs), INpcService
{
    private readonly IGameService _gameService = gameService;

    public async Task DeleteNpc(Guid id)
    {
        var npc = await GetNpc(id);
        await ThrowIfNull(
            id,
            npcId => Collection.FindOneAndDelete(npc => npc.NPCId == npcId),
            PropertyNames.NpcId);

        var game = await _gameService.GetGame(npc.GameId);
        game.NPCs.Remove(id);
        await _gameService.UpdateGameNpcList(npc.GameId, game.NPCs);
    }

    public async Task DeleteNpcByGameId(Guid gameId)
    {
        var game = await _gameService.GetGame(gameId);
        foreach (var npcId in game.NPCs)
        {
            await ThrowIfNull(
                gameId,
                gameId => Collection.FindOneAndDelete(npc => npc.NPCId == npcId),
                PropertyNames.GameId);
        }
        await _gameService.UpdateGameNpcList(gameId, []);
    }

    public async Task<NpcModel> GetNpc(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(npc => npc.NPCId == id).SingleOrDefault(),
            PropertyNames.NpcId);
    }

    public async Task<IEnumerable<NpcModel>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        return await ThrowIfNull(
            npcIds,
            id => Collection.Find(npc => npcIds.Contains(npc.NPCId)).ToEnumerable(),
            PropertyNames.NpcId);
    }

    public async Task<IEnumerable<NpcModel>> GetNpcsByGameId(Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(npc => npc.GameId == id).ToEnumerable(),
            PropertyNames.GameId);
    }

    public async Task PostNpc(NpcModel npc)
    {
        await PostDocument(npc);
    }

    public async Task<NpcModel> UpdateNpc(NpcModel updatedNpc)
    {
        await UpsertDocument(
            Builders<NpcModel>.Filter.Eq(npc => npc.NPCId, updatedNpc.NPCId),
            updatedNpc);

        return updatedNpc;
    }
}
