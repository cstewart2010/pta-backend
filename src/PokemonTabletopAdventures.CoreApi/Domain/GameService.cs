using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class GameService(
    ITrainerService trainerService,
    INpcService npcService,
    ISettingService settingService) : AbstractService<GameDto>(MongoCollection.Games), IGameService
{
    private readonly ITrainerService _trainerService = trainerService;
    private readonly INpcService _npcService = npcService;
    private readonly ISettingService _settingService = settingService;

    public async Task DeleteGame(Guid id)
    {
        await ThrowIfNull(
            id,
            gameId => Collection.FindOneAndDelete(game => game.GameId == gameId),
            PropertyNames.GameId);
    }

    public async Task<IEnumerable<Game>> GetAllGames(string nickname)
    {
        var dtos = await ThrowIfNull(
            nickname,
            name => Collection.Find(game => game.Nickname.Contains(name, StringComparison.CurrentCultureIgnoreCase)).ToEnumerable(),
            PropertyNames.Nickname);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<IEnumerable<Game>> GetAllGamesWithUser(User user)
    {
        var dtos = await ThrowIfNull(
            user.Games,
            games => Collection.Find(game => games.Contains(game.GameId)).ToEnumerable(),
            PropertyNames.UserGames);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<Game> GetGame(Guid id, bool isGM)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(game => game.GameId == id).SingleOrDefault(),
            PropertyNames.GameId);

        return await DtoHandler.ParseFromDto(dto, isGM, _npcService, _settingService, _trainerService);
    }

    public async Task<string> GetGameNickname(Guid gameId)
    {
        var game = await GetGame(gameId, false);
        return game.Nickname;
    }

    public async Task<IEnumerable<Game>> GetMostRecent20Games(User user)
    {
        var dtos = await ThrowIfNull(
            user.Games,
            games => Collection.Find(x => !games.Contains(x.GameId)).Limit(20).ToEnumerable(),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, false, _npcService, _settingService, _trainerService)));
    }

    public async Task<bool> HasGM(Guid gameId)
    {
        var trainers = await _trainerService.GetTrainersByGameId(gameId);
        var isGm = trainers.Any(trainer => trainer.IsGM);
        return await Task.FromResult(isGm);
    }

    public async Task PostGame(Game game, string passwordHash)
    {
        var dto = DtoHandler.ParseFromModel(game);
        dto.PasswordHash = passwordHash;
        await PostDocument(dto);
    }

    public async Task<Game> UpdateGameLogs(Game theGame, bool isGM, params Log[] logs)
    {
        var dto = await UpdateDocument(
            theGame.GameId,
            game => game.GameId == theGame.GameId,
            Builders<GameDto>.Update.Set(PropertyNames.Logs, theGame.Logs?.Union(logs) ?? logs));

        return await DtoHandler.ParseFromDto(dto, isGM, _npcService, _settingService, _trainerService);
    }

    public async Task<Game> UpdateGameNpcList(Guid gameId, IEnumerable<Guid> npcIds)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            Builders<GameDto>.Update.Set(PropertyNames.Npcs, npcIds));

        return await DtoHandler.ParseFromDto(dto, true, _npcService, _settingService, _trainerService);
    }

    public async Task<Game> UpdateGameOnlineStatus(Guid gameId, bool isOnline)
    {
        var dto = await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            Builders<GameDto>.Update.Set(PropertyNames.IsOnline, isOnline));

        return await DtoHandler.ParseFromDto(dto, true, _npcService, _settingService, _trainerService);
    }
}
