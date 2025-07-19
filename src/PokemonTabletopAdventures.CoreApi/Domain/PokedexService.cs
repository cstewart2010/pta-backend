using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokedex;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class PokedexService(IRepositoryService repositoryService) : AbstractMongoService<PokeDexItemDto>(repositoryService, MongoCollection.Pokedex), IPokedexService
{
    public async Task DeleteDexItemForTrainer(Guid trainerId, Guid gameId)
    {
        await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.DeleteAsync(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId),
            PropertyNames.TrainerId);
    }

    public async Task<PokedexItem> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await Collection.GetOneAsync(dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo)
             ?? throw new UnknownEntityException<PokedexItem>(PropertyNames.DexNo, dexNo);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<PokedexItem>> GetTrainerPokeDex(Guid trainerId, Guid gameId)
    {
        var dtos = await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.GetManyAsync(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId),
            $"{PropertyNames.TrainerId} {PropertyNames.GameId}");

        return dtos.Select(DtoHandler.ParseFromDto);
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

        await PostDocument(dexItem);
    }

    public async Task<PokedexItem> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            dexNo,
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            new Models.UpdateData(PropertyNames.IsSeen, true));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<PokedexItem> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            dexNo,
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            new Models.UpdateData(PropertyNames.IsSeen, true),
            new Models.UpdateData(PropertyNames.IsCaught, true));

        return DtoHandler.ParseFromDto(dto);
    }
}
