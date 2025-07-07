using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokedex;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class PokedexService : AbstractMongoService<PokeDexItemDto>, IPokedexService
{
    public PokedexService() : base(MongoCollection.Pokedex) { }

    public async Task DeleteDexItemForTrainer(Guid trainerId, Guid gameId)
    {
        await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.FindOneAndDelete(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId),
            PropertyNames.TrainerId);
    }

    public async Task<PokedexItem> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await Task.FromResult(Collection.Find(dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo).SingleOrDefault());

        return DtoHandler.ParseFromDto(dto);
        //return await ThrowIfNull(
        //    (trainerId, gameId, dexNo),
        //    x => Collection.Find(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId && dexItem.DexNo == dexNo).SingleOrDefault(),
        //    PropertyNames.DexNo);
    }

    public async Task<IEnumerable<PokedexItem>> GetTrainerPokeDex(Guid trainerId, Guid gameId)
    {
        var dtos = await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.Find(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId).ToEnumerable(),
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
            Builders<PokeDexItemDto>.Update.Set(PropertyNames.IsSeen, true));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<PokedexItem> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo)
    {
        var dto = await UpdateDocument(
            dexNo,
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            Builders<PokeDexItemDto>.Update.Set(PropertyNames.IsSeen, true),
            Builders<PokeDexItemDto>.Update.Set(PropertyNames.IsCaught, true));

        return DtoHandler.ParseFromDto(dto);
    }
}
