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
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<GameDto>(repositoryService, MongoCollection.Games), IGameService
{
    private readonly ITrainerService _trainerService = trainerService;
    private readonly INpcService _npcService = npcService;
    private readonly ISettingService _settingService = settingService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

    public async Task DeleteGame(Guid id)
    {
        await ThrowIfNull(
            id,
            gameId => Collection.DeleteAsync(game => game.GameId == gameId),
            PropertyNames.GameId);
    }

    public async Task<ICollection<Game>> GetAllGames(string nickname)
    {
        var dtos = await ThrowIfNullOrEmpty(
            nickname,
            name => Collection.GetManyAsync(game => game.Nickname.Contains(name, StringComparison.CurrentCultureIgnoreCase)),
            PropertyNames.Nickname);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<ICollection<Game>> GetAllGamesWithUser(User user)
    {
        var dtos = await ThrowIfNullOrEmpty(
            user.Games,
            games => Collection.GetManyAsync(game => games.Contains(game.GameId)),
            PropertyNames.UserGames);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<Game> GetGame(Guid id, bool isGM)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(game => game.GameId == id),
            PropertyNames.GameId);

        return await _dtoToModelMapper.ParseFromDto(dto, isGM, _npcService, _settingService, _trainerService);
    }

    public async Task<string> GetGameNickname(Guid gameId)
    {
        var game = await GetGame(gameId, false);
        return game.Nickname;
    }

    public async Task<ICollection<Game>> GetMostRecent20Games(User user)
    {
        var dtos = await ThrowIfNullOrEmpty(
            user.Games,
            games => Collection.GetManyAsync(x => !games.Contains(x.GameId), 0, 20),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<bool> HasGM(Guid gameId)
    {
        var trainers = await _trainerService.GetTrainersByGameId(gameId);
        var isGm = trainers.Any(trainer => trainer.IsGM);
        return await Task.FromResult(isGm);
    }

    public async Task PostGame(Game game, string passwordHash)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(game);
        dto.PasswordHash = passwordHash;
        await PostUniqueDocument(dto, x => x.GameId == game.GameId);
    }

    public async Task<Game> UpdateGameLogs(Game theGame, bool isGM, params Log[] logs)
    {
        var dto = await UpdateDocument(
            theGame.GameId,
            game => game.GameId == theGame.GameId,
            new Models.UpdateData(PropertyNames.Logs, theGame.Logs?.Union(logs) ?? logs));

        return await _dtoToModelMapper.ParseFromDto(dto, isGM, _npcService, _settingService, _trainerService);
    }

    public async Task<Game> UpdateGameNpcList(Guid gameId, ICollection<Guid> npcIds)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            new Models.UpdateData(PropertyNames.Npcs, npcIds));

        return await _dtoToModelMapper.ParseFromDto(dto, true, _npcService, _settingService, _trainerService);
    }

    public async Task<Game> UpdateGameOnlineStatus(Guid gameId, bool isOnline)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            new Models.UpdateData(PropertyNames.IsOnline, isOnline));

        return await _dtoToModelMapper.ParseFromDto(dto, true, _npcService, _settingService, _trainerService);
    }
}
