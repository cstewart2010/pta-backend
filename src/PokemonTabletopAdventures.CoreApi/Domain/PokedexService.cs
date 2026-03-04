using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokedex;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class PokedexService(
    IRepositoryService repositoryService,
    IDtoToModelMapper dtoToModelMapper,
    ILogger<PokedexService> logger) : AbstractMongoService<PokeDexItemDto>(repositoryService, MongoCollection.PokeDex), IPokedexService
{
    public async Task DeleteTrainerDex(Guid trainerId, Guid gameId)
    {
        logger.LogInformation("Deleting pokedex for trainer {trainerId}", trainerId);
        await Collection.DeleteManyAsync(dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId, logger);
    }

    public async Task<PokedexItem?> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await Collection.GetOneAsync(x => x.TrainerId == trainerId && x.GameId == gameId && x.DexNo == dexNo, logger);
        if (dto == null)
        {
            return null;
        }

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<ICollection<PokedexItem>> GetTrainerPokeDex(Guid trainerId, Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId, logger);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task PostDexItem(Guid trainerId, Guid gameId, int dexNo, bool isSeen, bool isCaught)
    {
        var dexItem = new PokeDexItemDto
        {
            TrainerId = trainerId,
            GameId = gameId,
            IsSeen = isSeen,
            IsCaught = isCaught,
            DexNo = dexNo
        };

        await PostUniqueDocument(dexItem, x => x.GameId == gameId && x.TrainerId == trainerId && x.DexNo == dexNo, logger);
    }

    public async Task<PokedexItem> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            (trainerId, gameId),
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            logger,
            new Models.UpdateData(PropertyNames.IsSeen, true),
            new Models.UpdateData(PropertyNames.IsCaught, true));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<PokedexItem> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            (trainerId, gameId),
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            logger,
            new Models.UpdateData(PropertyNames.IsSeen, true));

        return await dtoToModelMapper.ParseFromDto(dto);
    }
}
