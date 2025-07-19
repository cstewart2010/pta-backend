using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class NpcService(
    IRepositoryService repositoryService,
    IPokemonService pokemonService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<NpcDto>(repositoryService, MongoCollection.NPCs), INpcService
{
    private readonly IPokemonService _pokemonService = pokemonService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

    public async Task DeleteNpc(Guid id, IGameService gameService)
    {
        var npc = await GetNpc(id);
        await ThrowIfNull(
            id,
            npcId => Collection.DeleteAsync(npc => npc.NPCId == npcId),
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
                gameId => Collection.DeleteAsync(npc => npc.NPCId == npcModel.NpcId),
                PropertyNames.GameId);
        }
        await gameService.UpdateGameNpcList(gameId, []);
    }

    public async Task<Npc> GetNpc(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(npc => npc.NPCId == id),
            PropertyNames.NpcId);

        return await _dtoToModelMapper.ParseFromDto(dto, _pokemonService);
    }

    public async Task<IEnumerable<Npc>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var dtos = await ThrowIfNull(
            npcIds,
            id => Collection.GetManyAsync(npc => npcIds.Contains(npc.NPCId)),
            PropertyNames.NpcId);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, _pokemonService)));
    }

    public async Task<IEnumerable<Npc>> GetNpcsByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.GetManyAsync(npc => npc.GameId == id),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, _pokemonService)));
    }

    public async Task PostNpc(Npc npc)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(npc);
        await PostDocument(dto);
    }

    public async Task<Npc> UpdateNpc(Npc updatedNpc)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(updatedNpc);
        await UpsertDocument(
            npc => npc.NPCId,
            updatedNpc.NpcId,
            dto);

        return await GetNpc(dto.NPCId);
    }
}
