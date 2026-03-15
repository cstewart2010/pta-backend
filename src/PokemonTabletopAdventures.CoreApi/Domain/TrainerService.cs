using PokemonTabletopAdventures.CoreApi.Constants;
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
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<TrainerService> logger) : AbstractMongoService<TrainerDto>(repositoryService, MongoCollection.Trainers), ITrainerService
{
    private readonly IRepositoryService _repositoryService = repositoryService;

    public async Task<Trainer> CompleteTrainer(
        Guid trainerId,
        Guid gameId,
        string origin,
        string trainerClass,
        IEnumerable<string> feats,
        Stats stats)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            logger,
            new UpdateData(nameof(Trainer.Origin), origin),
            new UpdateData(nameof(Trainer.TrainerClasses), new[] { trainerClass }),
            new UpdateData(nameof(Trainer.Feats), feats),
            new UpdateData(nameof(Trainer.TrainerStats), stats),
            new UpdateData(nameof(Trainer.IsComplete), true));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task DeleteTrainer(Guid userId, Guid gameId)
    {
        logger.LogInformation("Deleting trainer {id} from game {gameId}", userId, gameId);
        var trainer = await ThrowIfNull(
            userId,
            id => Collection.DeleteAsync(x => x.TrainerId == id && x.GameId == gameId, logger),
            nameof(Trainer.TrainerId));

        if (trainer.IsGM)
        {
            var gameCollection = _repositoryService.GetCollection<GameDto>(MongoCollection.Games);
            await gameCollection.DeleteAsync(x => x.GameId == trainer.GameId, logger);
            var pokemonCollection = _repositoryService.GetCollection<PokemonDto>(MongoCollection.Pokemon);
            await pokemonCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
            var trainerCollection = _repositoryService.GetCollection<TrainerDto>(MongoCollection.Trainers);
            await trainerCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
            var settingCollection = _repositoryService.GetCollection<SettingDto>(MongoCollection.Settings);
            await settingCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
            var shopCollection = _repositoryService.GetCollection<ShopDto>(MongoCollection.Shops);
            await shopCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
            var npcCollection = _repositoryService.GetCollection<NpcDto>(MongoCollection.NPCs);
            await npcCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
            var pokedexCollection = _repositoryService.GetCollection<PokeDexItemDto>(MongoCollection.PokeDex);
            await pokedexCollection.DeleteManyAsync(x => x.GameId == trainer.GameId, logger);
        }
        else
        {
            await pokemonService.DeletePokemonByTrainerId(gameId, userId);
            await pokedexService.DeleteTrainerDex(userId, gameId);
            var settings = await settingService.GetAllSettings(gameId);
            foreach (var setting in settings)
            {
                setting.Participants = [..setting.Participants.Where(x => x.ParticipantId != userId)];
                await settingService.UpdateSetting(setting, true);
            }
        }

        await UpdateUserAfterTrainerDeletion(gameId, userId);
    }

    public async Task<Trainer> GetIncompleteTrainerById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (id, gameId),
            x => Collection.GetOneAsync(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId && !trainer.IsComplete, logger),
            nameof(Trainer.IsComplete));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<Trainer> GetTrainerById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            (id, gameId),
            x => Collection.GetOneAsync(trainer => trainer.TrainerId == x.id && trainer.GameId == x.gameId, logger),
            nameof(Trainer.TrainerId));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<Trainer?> GetTrainerByUsername(string username, Guid gameId)
    {
        var dto = await Collection.GetOneAsync(x => x.TrainerName.Equals(username, StringComparison.CurrentCultureIgnoreCase) && x.GameId == gameId, logger);
        if (dto == null)
        {
            return null;
        }

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<ICollection<Trainer>> GetTrainersByGameId(Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(trainer => trainer.GameId == gameId, logger);
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService)));
    }

    public async Task PostTrainer(Trainer trainer)
    {
        var dto = await modelToDtoMapper.ParseFromModel(trainer);
        await PostUniqueDocument(dto, x => trainer.TrainerId == x.TrainerId && trainer.GameId == x.GameId, logger);
    }

    public async Task<Trainer> UpdateTrainer(Trainer updatedTrainer)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedTrainer);
        await UpsertDocument(
            trainer => trainer.TrainerId,
            updatedTrainer.TrainerId,
            dto,
            logger);

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
            logger,
            new UpdateData(nameof(Trainer.Honors), honors));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<Trainer> UpdateTrainerItemList(
        Guid trainerId,
        Guid gameId,
        IEnumerable<Item> itemList)
    {
        var itemListDto = await Task.WhenAll(itemList.Select(modelToDtoMapper.ParseFromModel));
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            logger,
            new UpdateData(nameof(Trainer.Items), itemListDto));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<Trainer> UpdateTrainerOnlineStatus(
        Guid trainerId,
        Guid gameId,
        bool isOnline)
    {
        var dto = await UpdateDocument(
            trainerId,
            trainer => trainer.TrainerId == trainerId && trainer.GameId == gameId,
            logger,
            TrainerStatusUpdate(isOnline));

        return await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService);
    }

    public async Task<ICollection<Trainer>> GetAllUserTrainers(Guid userId)
    {
        var dtos = await  Collection.GetManyAsync(trainer => trainer.TrainerId == userId, logger);
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, pokemonService, pokedexService)));
    }

    private async Task UpdateUserAfterTrainerDeletion(Guid gameId, Guid userId)
    {
        var userCollection = _repositoryService.GetCollection<UserDto>(MongoCollection.Users);
        var user = await userCollection.GetOneAsync(x => x.UserId == userId, logger);
        if (user != null)
        {
            user.Games.Remove(gameId);
        
            await userCollection.PutAsync(
                x => x.UserId,
                user.UserId,
                user,
                logger);
        }
    }

    private static UpdateData[] TrainerStatusUpdate(bool isOnline)
    {
        return [new UpdateData(nameof(Trainer.IsOnline), isOnline)];
    }}
