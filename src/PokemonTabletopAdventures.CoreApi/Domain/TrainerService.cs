using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class TrainerService(
    IRepositoryService repositoryService,
    IPokemonService pokemonService,
    IPokedexService pokedexService,
    ISettingService settingService,
    IEncryptionService encryptionService) : AbstractMongoService<TrainerDto>(repositoryService, MongoCollection.Trainers), ITrainerService
{
    private readonly IRepositoryService _repositoryService = repositoryService;
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
            new UpdateData(PropertyNames.Origin, origin),
            new UpdateData(PropertyNames.TrainerClasses, new[] { trainerClass }),
            new UpdateData(PropertyNames.Feats, feats),
            new UpdateData(PropertyNames.TrainerStats, stats),
            new UpdateData(PropertyNames.IsComplete, true));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task DeleteTrainer(Guid gameId, Guid userId)
    {
        var trainer = await ThrowIfNull(
            userId,
            id => Collection.DeleteAsync(x => x.TrainerId == id && x.GameId == gameId),
            PropertyNames.TrainerId);

        if (trainer.IsGM)
        {
            var gameCollection = _repositoryService.GetCollection<GameDto>(MongoCollection.Games);
            await gameCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var pokemonCollection = _repositoryService.GetCollection<PokemonDto>(MongoCollection.Pokemon);
            await pokemonCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var trainerCollection = _repositoryService.GetCollection<TrainerDto>(MongoCollection.Trainers);
            await trainerCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var settingCollection = _repositoryService.GetCollection<SettingDto>(MongoCollection.Settings);
            await settingCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var shopCollection = _repositoryService.GetCollection<ShopDto>(MongoCollection.Shops);
            await shopCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var npcCollection = _repositoryService.GetCollection<NpcDto>(MongoCollection.NPCs);
            await npcCollection.DeleteAsync(x => x.GameId == trainer.GameId);
            var pokedexCollection = _repositoryService.GetCollection<NpcDto>(MongoCollection.Pokedex);
            await pokedexCollection.DeleteAsync(x => x.GameId == trainer.GameId);
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

        await UpdateUserAfterTrainerDeletion(gameId, userId);
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
            x => Collection.GetOneAsync(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId && !trainer.IsComplete),
            PropertyNames.IsComplete);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> GetTrainerById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (id, gameId),
            x => Collection.GetOneAsync(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId),
            PropertyNames.TrainerId);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<Trainer> GetTrainerByUsername(string username, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (username, gameId),
            x => Collection.GetOneAsync(trainer => trainer.TrainerName.Equals(x.username, StringComparison.CurrentCultureIgnoreCase) && trainer.GameId == x.gameId),
            PropertyNames.TrainerId);

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<IEnumerable<Trainer>> GetTrainersByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.GetManyAsync(trainer => trainer.GameId == id),
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
            trainer => trainer.TrainerId,
            updatedTrainer.TrainerId,
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
            new UpdateData(PropertyNames.Honors, honors));

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
            new UpdateData(PropertyNames.Items, itemList));

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
            new UpdateData(PropertyNames.PasswordHash, passwordHash),
            new UpdateData(PropertyNames.IsOnline, true));

        return await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService);
    }

    public async Task<IEnumerable<Trainer>> GetAllUserTrainers(Guid userId)
    {
        var dtos = await  ThrowIfNull(
            userId,
            id => Collection.GetManyAsync(trainer => trainer.TrainerId == id),
            PropertyNames.TrainerId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, _pokemonService, _pokedexService)));
    }

    private async Task UpdateUserAfterTrainerDeletion(Guid gameId, Guid userId)
    {
        var userCollection = _repositoryService.GetCollection<UserDto>(MongoCollection.Users);
        var user = await userCollection.GetOneAsync(x => x.UserId == userId);
        user.Games.Remove(gameId);
        
        await userCollection.PutAsync(
            user => user.UserId,
            user.UserId,
            user);
    }

    private static UpdateData[] TrainerStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return [new UpdateData(PropertyNames.IsOnline, isOnline)];
        }

        return
        [
            new UpdateData(PropertyNames.IsOnline, isOnline),
            new UpdateData(PropertyNames.ActivityToken, string.Empty)
        ];
    }}
