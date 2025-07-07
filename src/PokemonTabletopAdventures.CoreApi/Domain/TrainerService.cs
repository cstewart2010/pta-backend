using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class TrainerService(
    IPokemonService pokemonService,
    IPokedexService pokedexService,
    ISettingService settingService,
    IEncryptionService encryptionService) : AbstractMongoService<TrainerDto>(MongoCollection.Trainers), ITrainerService
{
    private readonly IPokemonService _pokemonService = pokemonService;
    private readonly IPokedexService _pokedexService = pokedexService;
    private readonly ISettingService _settingService = settingService;
    private readonly IEncryptionService _encryptionService = encryptionService;

    public async Task<Trainer> CompleteTrainer(
        Guid trainerId,
        string origin,
        string trainerClass,
        IEnumerable<string> feats,
        Stats stats)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId,
            Builders<TrainerDto>.Update.Set(PropertyNames.Origin, origin),
            Builders<TrainerDto>.Update.Set(PropertyNames.TrainerClasses, new[] { trainerClass }),
            Builders<TrainerDto>.Update.Set(PropertyNames.Feats, feats),
            Builders<TrainerDto>.Update.Set(PropertyNames.TrainerStats, stats),
            Builders<TrainerDto>.Update.Set(PropertyNames.IsComplete, true));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task DeleteTrainer(Guid gameId, Guid userId)
    {
        var trainer = await ThrowIfNull(
            userId,
            id => Collection.FindOneAndDelete(x => x.TrainerId == id && x.GameId == gameId),
            PropertyNames.TrainerId);

        if (trainer.IsGM)
        {
            var gameCollection = MongoCollectionHelper.GetMongoCollection<GameDto>(MongoCollection.Games);
            gameCollection.FindOneAndDelete(x => x.GameId == trainer.GameId);
            var pokemonCollection = MongoCollectionHelper.GetMongoCollection<PokemonDto>(MongoCollection.Pokemon);
            pokemonCollection.DeleteMany(x => x.GameId == trainer.GameId);
            var trainerCollection = MongoCollectionHelper.GetMongoCollection<TrainerDto>(MongoCollection.Trainers);
            trainerCollection.DeleteMany(x => x.GameId == trainer.GameId);
            var settingCollection = MongoCollectionHelper.GetMongoCollection<SettingDto>(MongoCollection.Settings);
            settingCollection.DeleteMany(x => x.GameId == trainer.GameId);
            var shopCollection = MongoCollectionHelper.GetMongoCollection<ShopDto>(MongoCollection.Shops);
            shopCollection.DeleteMany(x => x.GameId == trainer.GameId);
            var npcCollection = MongoCollectionHelper.GetMongoCollection<NpcDto>(MongoCollection.NPCs);
            npcCollection.DeleteMany(x => x.GameId == trainer.GameId);
            var pokedexCollection = MongoCollectionHelper.GetMongoCollection<NpcDto>(MongoCollection.Pokedex);
            pokedexCollection.DeleteMany(x => x.GameId == trainer.GameId);
        }
        else
        {
            await _pokemonService.DeletePokemonByTrainerId(gameId, userId);
            await _pokedexService.DeleteDexItemForTrainer(userId, gameId);
            var settings = await _settingService.GetAllSettings(gameId);
            foreach (var setting in settings)
            {
                setting.Participants = [..setting.Participants.Where(x => x.ParticipantId != userId)];
                await _settingService.UpdateSetting(setting, false);
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

    public async Task<Trainer> GetIncompleteTrainerById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (id, gameId),
            x => Collection.Find(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId && !trainer.IsComplete).SingleOrDefault(),
            PropertyNames.IsComplete);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> GetTrainerById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (id, gameId),
            x => Collection.Find(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId).SingleOrDefault(),
            PropertyNames.TrainerId);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> GetTrainerByUsername(string username, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (username, gameId),
            x => Collection.Find(trainer => trainer.TrainerName.Equals(x.username, StringComparison.CurrentCultureIgnoreCase) && trainer.GameId == x.gameId).SingleOrDefault(),
            PropertyNames.TrainerId);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<IEnumerable<Trainer>> GetTrainersByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.Find(trainer => trainer.GameId == id).ToEnumerable(),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService)));
    }

    public async Task PostTrainer(Trainer trainer)
    {
        var dto = DtoHandler.ParseFromModel(trainer);
        await PostDocument(dto);
    }

    public async Task<Trainer> UpdateTrainer(Trainer updatedTrainer)
    {
        var dto = DtoHandler.ParseFromModel(updatedTrainer);
        await UpsertDocument(
            Builders<TrainerDto>.Filter.Eq(trainer => trainer.TrainerId, updatedTrainer.TrainerId),
            dto);

        return updatedTrainer;
    }

    public async Task<Trainer> UpdateTrainerHonors(
        Guid trainerId,
        Guid gameId,
        IEnumerable<string> honors)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerDto>.Update.Set(PropertyNames.Honors, honors));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> UpdateTrainerItemList(
        Guid trainerId,
        Guid gameId,
        IEnumerable<Item> itemList)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerDto>.Update.Set(PropertyNames.Items, itemList));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> UpdateTrainerOnlineStatus(
        Guid trainerId,
        Guid gameId,
        bool isOnline)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            TrainerStatusUpdate(isOnline));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> UpdateTrainerPassword(
        Guid trainerId,
        Guid gameId,
        string password)
    {
        var passwordHash = _encryptionService.HashSecret(password);
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            Builders<TrainerDto>.Update.Set(PropertyNames.PasswordHash, passwordHash),
            Builders<TrainerDto>.Update.Set(PropertyNames.IsOnline, true));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<IEnumerable<Trainer>> GetAllUserTrainers(Guid userId)
    {
        var dtos = await  ThrowIfNull(
            userId,
            id => Collection.Find(trainer => trainer.TrainerId == id).ToEnumerable(),
            PropertyNames.TrainerId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService)));
    }

    private static void UpdateUserAfterTrainerDeletion(Guid gameId, Guid userId)
    {
        var userCollection = MongoCollectionHelper.GetMongoCollection<UserDto>(MongoCollection.Users);
        var user = userCollection.Find(x => x.UserId == userId).First();
        user.Games.Remove(gameId);
        userCollection.ReplaceOne(x => x.UserId == userId, options: new ReplaceOptions { IsUpsert = true }, replacement: user);
    }

    private static UpdateDefinition<TrainerDto> TrainerStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return Builders<TrainerDto>.Update.Set(PropertyNames.IsOnline, isOnline);
        }

        return Builders<TrainerDto>.Update.Combine(
            Builders<TrainerDto>.Update.Set(PropertyNames.IsOnline, isOnline),
            Builders<TrainerDto>.Update.Set(PropertyNames.ActivityToken, string.Empty));
    }}
