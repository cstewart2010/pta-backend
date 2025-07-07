using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class NpcService(
    IPokemonService pokemonService) : AbstractService<NpcDto>(MongoCollection.NPCs), INpcService
{
    private readonly IPokemonService _pokemonService = pokemonService;

    public async Task DeleteNpc(Guid id, IGameService gameService)
    {
        var npc = await GetNpc(id);
        await ThrowIfNull(
            id,
            npcId => Collection.FindOneAndDelete(npc => npc.NPCId == npcId),
            PropertyNames.NpcId);

        var game = await gameService.GetGame(npc.GameId, true);
        var npcList = game.Npcs.Select(npc => npc.NpcId).Where(npcId => id != npcId);
        await gameService.UpdateGameNpcList(npc.GameId, npcList);
    }

    public async Task DeleteNpcByGameId(Guid gameId, IGameService gameService)
    {
        var game = await gameService.GetGame(gameId, true);
        foreach (var npcModel in game.Npcs)
        {
            await ThrowIfNull(
                gameId,
                gameId => Collection.FindOneAndDelete(npc => npc.NPCId == npcModel.NpcId),
                PropertyNames.GameId);
        }
        await gameService.UpdateGameNpcList(gameId, []);
    }

    public async Task<Npc> GetNpc(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(npc => npc.NPCId == id).SingleOrDefault(),
            PropertyNames.NpcId);

        return await DtoHandler.ParseFromDto(dto, _pokemonService);
    }

    public async Task<IEnumerable<Npc>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var dtos = await ThrowIfNull(
            npcIds,
            id => Collection.Find(npc => npcIds.Contains(npc.NPCId)).ToEnumerable(),
            PropertyNames.NpcId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, _pokemonService)));
    }

    public async Task<IEnumerable<Npc>> GetNpcsByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.Find(npc => npc.GameId == id).ToEnumerable(),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, _pokemonService)));
    }

    public async Task PostNpc(Npc npc)
    {
        var dto = DtoHandler.ParseFromModel(npc);
        await PostDocument(dto);
    }

    public async Task<Npc> UpdateNpc(Npc updatedNpc)
    {
        var dto = DtoHandler.ParseFromModel(updatedNpc);
        await UpsertDocument(
            Builders<NpcDto>.Filter.Eq(npc => npc.NPCId, updatedNpc.NpcId),
            dto);

        return await GetNpc(dto.NPCId);
    }
}
