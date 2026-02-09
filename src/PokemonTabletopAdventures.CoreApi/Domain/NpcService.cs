using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class NpcService(
    IRepositoryService repositoryService,
    IPokemonService pokemonService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<NpcService> logger) : AbstractMongoService<NpcDto>(repositoryService, MongoCollection.NPCs), INpcService
{
    private readonly IPokemonService _pokemonService = pokemonService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;
    private readonly ILogger<NpcService> _logger = logger;

    public async Task DeleteNpc(Guid id)
    {
        await ThrowIfNull(
            id,
            npcId => Collection.DeleteAsync(npc => npc.NPCId == npcId),
            PropertyNames.NpcId);
    }

    public async Task<Npc> GetNpc(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(npc => npc.NPCId == id),
            PropertyNames.NpcId);

        return await _dtoToModelMapper.ParseFromDto(dto, _pokemonService);
    }

    public async Task<ICollection<Npc>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var dtos = await ThrowIfNull(
            npcIds,
            id => Collection.GetManyAsync(npc => npcIds.Contains(npc.NPCId)),
            PropertyNames.NpcId);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, _pokemonService)));
    }

    public async Task<ICollection<Npc>> GetNpcsByGameId(Guid gameId)
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
        await PostUniqueDocument(dto, x => x.GameId == npc.GameId && x.NPCId == npc.NpcId);
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
