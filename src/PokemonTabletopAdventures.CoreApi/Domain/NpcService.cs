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
    public async Task DeleteNpc(Guid id)
    {
        logger.LogInformation("Deleting Npc {id}", id);
        await ThrowIfNull(
            id,
            npcId => Collection.DeleteAsync(npc => npc.NPCId == npcId),
            PropertyNames.NpcId);
    }

    public async Task<Npc> GetNpc(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            x => Collection.GetOneAsync(npc => npc.NPCId == x),
            PropertyNames.NpcId);

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService);
    }

    public async Task<ICollection<Npc>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var dtos = await Collection.GetManyAsync(npc => npcIds.Contains(npc.NPCId));
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, pokemonService)));
    }

    public async Task<ICollection<Npc>> GetNpcsByGameId(Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(npc => npc.GameId == gameId);
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, pokemonService)));
    }

    public async Task PostNpc(Npc npc)
    {
        var dto = await modelToDtoMapper.ParseFromModel(npc);
        await PostUniqueDocument(dto, x => x.GameId == npc.GameId && x.NPCId == npc.NpcId);
    }

    public async Task<Npc> UpdateNpc(Npc updatedNpc)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedNpc);
        await UpsertDocument(
            npc => npc.NPCId,
            updatedNpc.NpcId,
            dto);

        return await GetNpc(dto.NPCId);
    }
}
