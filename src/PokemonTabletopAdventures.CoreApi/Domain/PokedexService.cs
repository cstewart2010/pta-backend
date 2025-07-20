using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokedex;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class PokedexService(
    IRepositoryService repositoryService,
    IDtoToModelMapper dtoToModelMapper,
    ILogger<PokedexService> logger) : AbstractMongoService<PokeDexItemDto>(repositoryService, MongoCollection.Pokedex), IPokedexService
{
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly ILogger<PokedexService> _logger = logger;

    public async Task DeleteDexItemForTrainer(Guid trainerId, Guid gameId)
    {
        await Collection.DeleteManyAsync(dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId);
    }

    public async Task<PokedexItem> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await ThrowIfNull(
            (trainerId, gameId, dexNo),
            x => Collection.GetOneAsync(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId && dexItem.DexNo == x.dexNo),
            $"{PropertyNames.TrainerId} {PropertyNames.GameId} {PropertyNames.DexNo}");

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<IEnumerable<PokedexItem>> GetTrainerPokeDex(Guid trainerId, Guid gameId)
    {
        var dtos = await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.GetManyAsync(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId),
            $"{PropertyNames.TrainerId} {PropertyNames.GameId}");

        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
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

        await PostUniqueDocument(dexItem, x => x.GameId == gameId && x.TrainerId == trainerId && x.DexNo == dexNo);
    }

    public async Task<PokedexItem> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            (trainerId, gameId),
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            new Models.UpdateData(PropertyNames.IsSeen, true),
            new Models.UpdateData(PropertyNames.IsCaught, true));

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<PokedexItem> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            (trainerId, gameId),
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            new Models.UpdateData(PropertyNames.IsSeen, true));

        return await _dtoToModelMapper.ParseFromDto(dto);
    }
}
