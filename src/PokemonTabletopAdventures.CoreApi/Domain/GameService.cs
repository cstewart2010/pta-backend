using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class GameService(
    IRepositoryService repositoryService,
    ITrainerService trainerService,
    INpcService npcService,
    ISettingService settingService,
    IShopService shopService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<GameService> logger) : AbstractMongoService<GameDto>(repositoryService, MongoCollection.Games), IGameService
{
    public async Task DeleteGame(Guid id)
    {
        logger.LogInformation("Deleting game {id}", id);
        await ThrowIfNull(
            id,
            gameId => Collection.DeleteAsync(game => game.GameId == gameId),
            PropertyNames.GameId);

        logger.LogInformation("Deleting relevant items to game {id}", id);
        await settingService.DeleteSettingsByGameId(id);
        await shopService.DeleteShopByGameId(id);
        var trainers = await trainerService.GetTrainersByGameId(id);
        await Task.WhenAll(trainers.Select(trainer => trainerService.DeleteTrainer(trainer.TrainerId, id)));
    }

    public async Task<ICollection<Game>> GetAllGames(string nickname)
    {
        var dtos = await Collection.GetManyAsync(game =>
            game.Nickname.Contains(nickname, StringComparison.CurrentCultureIgnoreCase));
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, false, npcService, settingService, trainerService)));
    }

    public async Task<ICollection<Game>> GetAllGamesWithUser(User user)
    {
        var dtos = await Collection.GetManyAsync(game => user.Games.Contains(game.GameId));
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, false, npcService, settingService, trainerService)));
    }

    public async Task<Game> GetGame(Guid id, bool isGM)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(game => game.GameId == id),
            PropertyNames.GameId);

        return await dtoToModelMapper.ParseFromDto(dto, isGM, npcService, settingService, trainerService);
    }

    public async Task<string> GetGameNickname(Guid gameId)
    {
        var game = await GetGame(gameId, false);
        return game.Nickname;
    }

    public async Task<ICollection<Game>> GetMostRecent20Games(User user)
    {
        var dtos = await Collection.GetManyAsync(x => !user.Games.Contains(x.GameId), 0, 20);

        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, false, npcService, settingService, trainerService)));
    }

    public async Task<bool> HasGM(Guid gameId)
    {
        var trainers = await trainerService.GetTrainersByGameId(gameId);
        var isGm = trainers.Any(trainer => trainer.IsGM);
        return await Task.FromResult(isGm);
    }

    public async Task PostGame(Game game, string passwordHash)
    {
        var dto = await modelToDtoMapper.ParseFromModel(game);
        dto.PasswordHash = passwordHash;
        await PostUniqueDocument(dto, x => x.GameId == game.GameId);
    }

    public async Task<Game> UpdateGameLogs(Game theGame, bool isGM, params Log[] logs)
    {
        var dto = await UpdateDocument(
            theGame.GameId,
            game => game.GameId == theGame.GameId,
            new Models.UpdateData(PropertyNames.Logs, theGame.Logs?.Union(logs) ?? logs));

        return await dtoToModelMapper.ParseFromDto(dto, isGM, npcService, settingService, trainerService);
    }

    public async Task<Game> UpdateGameNpcList(Guid gameId, ICollection<Guid> npcIds)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            new Models.UpdateData(PropertyNames.Npcs, npcIds));

        return await dtoToModelMapper.ParseFromDto(dto, true, npcService, settingService, trainerService);
    }

    public async Task<Game> UpdateGameOnlineStatus(Guid gameId, bool isOnline)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            new Models.UpdateData(PropertyNames.IsOnline, isOnline));

        return await dtoToModelMapper.ParseFromDto(dto, true, npcService, settingService, trainerService);
    }
}
