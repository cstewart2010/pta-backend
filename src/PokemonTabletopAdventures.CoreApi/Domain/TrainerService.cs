using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class TrainerService(
    IPokemonService pokemonService,
    IPokedexService pokedexService,
    ISettingService settingService,
    IEncryptionService encryptionService) : AbstractService<TrainerModel>(MongoCollection.Trainers), ITrainerService
{
    private readonly IPokemonService _pokemonService = pokemonService;
    private readonly IPokedexService _pokedexService = pokedexService;
    private readonly ISettingService _settingService = settingService;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<TrainerModel> CompleteTrainer(
        Guid trainerId,
        string origin,
        string trainerClass,
        IEnumerable<string> feats,
        StatsModel stats)
    {
        return await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId,
            Builders<TrainerModel>.Update.Set("Origin", origin),
            Builders<TrainerModel>.Update.Set("TrainerClasses", new[] { trainerClass }),
            Builders<TrainerModel>.Update.Set("Feats", feats),
            Builders<TrainerModel>.Update.Set("TrainerStats", stats),
            Builders<TrainerModel>.Update.Set("IsComplete", true));
    }

    public async Task DeleteTrainer(Guid gameId, Guid userId)
    {
        var trainer = await ThrowIfNull(
            userId,
            id => Collection.FindOneAndDelete(x => x.TrainerId == id && x.GameId == gameId),
            "TrainerId");

        if (trainer.IsGM)
        {
            var gameCollection = MongoCollectionHelper.GetMongoCollection<GameModel>(MongoCollection.Games);
            gameCollection.FindOneAndDelete(x => x.GameId == trainer.GameId);
        }
        else
        {
            await _pokemonService.DeletePokemonByTrainerId(gameId, userId);
            await _pokedexService.DeleteDexItemForTrainer(userId, gameId);
            var settings = await _settingService.GetAllSettings(gameId);
            foreach (var setting in settings)
            {
                setting.ActiveParticipants = setting.ActiveParticipants.Where(x => x.ParticipantId != userId);
                await _settingService.UpdateSetting(setting);
            }
        }

        UpdateUserAfterTrainerDeletion(gameId, userId);
    }

    public async Task DeleteTrainersByGameId(Guid gameId)
    {
        foreach (var trainer in await GetTrainersByGameId(gameId))
        {
            await DeleteTrainer(gameId, trainer.TrainerId);
        }
    }

    public async Task<TrainerModel> GetIncompleteTrainerById(Guid id, Guid gameId)
    {
        return await ThrowIfNull(
            (id, gameId),
            x => Collection.Find(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId && !trainer.IsComplete).SingleOrDefault(),
            "IsComplete");
    }

    public async Task<TrainerModel> GetTrainerById(Guid id, Guid gameId)
    {
        return await ThrowIfNull(
            (id, gameId),
            x => Collection.Find(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId).SingleOrDefault(),
            "TrainerId");
    }

    public async Task<TrainerModel> GetTrainerByUsername(string username, Guid gameId)
    {
        return await ThrowIfNull(
            (username, gameId),
            x => Collection.Find(trainer => trainer.TrainerName.Equals(x.username, StringComparison.CurrentCultureIgnoreCase) && trainer.GameId == x.gameId).SingleOrDefault(),
            "TrainerId");
    }

    public async Task<IEnumerable<TrainerModel>> GetTrainersByGameId(Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(trainer => trainer.GameId == id).ToEnumerable(),
            "GameId");
    }

    public async Task PostTrainer(TrainerModel trainer)
    {
        await PostDocument(trainer);
    }

    public async Task<TrainerModel> UpdateTrainer(TrainerModel updatedTrainer)
    {
       await  UpsertDocument(
            Builders<TrainerModel>.Filter.Eq(trainer => trainer.TrainerId, updatedTrainer.TrainerId),
            updatedTrainer);

        return updatedTrainer;
    }

    public async Task<TrainerModel> UpdateTrainerHonors(
        Guid trainerId,
        Guid gameId,
        IEnumerable<string> honors)
    {
        return await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerModel>.Update.Set("Honors", honors));
    }

    public async Task<TrainerModel> UpdateTrainerItemList(
        Guid trainerId,
        Guid gameId,
        IEnumerable<ItemModel> itemList)
    {
        return await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerModel>.Update.Set("Items", itemList));
    }

    public async Task<TrainerModel> UpdateTrainerOnlineStatus(
        Guid trainerId,
        Guid gameId,
        bool isOnline)
    {
        return await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            TrainerStatusUpdate(isOnline));
    }

    public async Task<TrainerModel> UpdateTrainerPassword(
        Guid trainerId,
        Guid gameId,
        string password)
    {
        var passwordHash = _encryptionService.HashSecret(password);
        return await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerModel>.Update.Set("Items", passwordHash),
            Builders<TrainerModel>.Update.Set("IsOnline", true));
    }

    public async Task<IEnumerable<TrainerModel>> GetAllUserTrainers(Guid userId)
    {
        return await  ThrowIfNull(
            userId,
            id => Collection.Find(trainer => trainer.TrainerId == id).ToEnumerable(),
            "TrainerId");
    }

    private static void UpdateUserAfterTrainerDeletion(Guid gameId, Guid userId)
    {
        var userCollection = MongoCollectionHelper.GetMongoCollection<UserModel>(MongoCollection.Users);
        var user = userCollection.Find(x => x.UserId == userId).First();
        user.Games.Remove(gameId);
        userCollection.ReplaceOne(x => x.UserId == userId, options: new ReplaceOptions { IsUpsert = true }, replacement: user);
    }

    private static UpdateDefinition<TrainerModel> TrainerStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return Builders<TrainerModel>.Update.Set("IsOnline", isOnline);
        }

        return Builders<TrainerModel>.Update.Combine(
            Builders<TrainerModel>.Update.Set("IsOnline", isOnline),
            Builders<TrainerModel>.Update.Set("ActivityToken", string.Empty));
    }
}
