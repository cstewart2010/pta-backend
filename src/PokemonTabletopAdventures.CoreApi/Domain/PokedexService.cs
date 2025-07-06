using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class PokedexService : AbstractService<PokeDexItemModel>, IPokedexService
{
    public PokedexService() : base(MongoCollection.Pokedex) { }

    public async Task DeleteDexItemForTrainer(Guid trainerId, Guid gameId)
    {
        await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.FindOneAndDelete(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId),
            PropertyNames.TrainerId);
    }

    public async Task<PokeDexItemModel> GetPokedexItem(Guid trainerId, Guid gameId, int dexNo)
    {
        return await ThrowIfNull(
            (trainerId, gameId, dexNo),
            x => Collection.Find(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId && dexItem.DexNo == dexNo).SingleOrDefault(),
            PropertyNames.DexNo);
    }

    public async Task<IEnumerable<PokeDexItemModel>> GetTrainerPokeDex(Guid trainerId, Guid gameId)
    {
        return await ThrowIfNull(
            (trainerId, gameId),
            x => Collection.Find(dexItem => dexItem.TrainerId == x.trainerId && dexItem.GameId == x.gameId).ToEnumerable(),
            $"{PropertyNames.TrainerId} {PropertyNames.GameId}");
    }

    public async Task PostDexItem(Guid trainerId, Guid gameId, int dexNo, bool isSeen, bool isCaught)
    {
        var dexItem = new PokeDexItemModel
        {
            TrainerId = trainerId,
            GameId = gameId,
            IsSeen = isSeen,
            IsCaught = isCaught,
            DexNo = dexNo
        };

        await PostDocument(dexItem);
    }

    public async Task<PokeDexItemModel> UpdateDexItemIsCaught(Guid trainerId, Guid gameId, int dexNo)
    {
        return await UpdateDocument(
            dexNo,
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            Builders<PokeDexItemModel>.Update.Set(PropertyNames.IsSeen, true));
    }

    public async Task<PokeDexItemModel> UpdateDexItemIsSeen(Guid trainerId, Guid gameId, int dexNo)
    {
        return await UpdateDocument(
            dexNo,
            dexItem => dexItem.TrainerId == trainerId && dexItem.GameId == gameId && dexItem.DexNo == dexNo,
            Builders<PokeDexItemModel>.Update.Set(PropertyNames.IsSeen, true),
            Builders<PokeDexItemModel>.Update.Set(PropertyNames.IsCaught, true));
    }
}
