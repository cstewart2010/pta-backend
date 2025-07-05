using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class GameService(ITrainerService trainerService) : AbstractService<GameModel>(MongoCollection.Games), IGameService
{
    private readonly ITrainerService _trainerService = trainerService;

    public async Task DeleteGame(Guid id)
    {
        await ThrowIfNull(
            id,
            gameId => Collection.FindOneAndDelete(game => game.GameId == gameId),
            "GameId");
    }

    public async Task<IEnumerable<GameModel>> GetAllGames(string nickname)
    {
        return await ThrowIfNull(
            nickname,
            name => Collection.Find(game => !game.Nickname.Contains(name, StringComparison.CurrentCultureIgnoreCase)).ToEnumerable(),
            "Nickname");
    }

    public async Task<IEnumerable<GameModel>> GetAllGamesWithUser(UserModel user)
    {
        return await ThrowIfNull(
            user.Games,
            games => Collection.Find(game => !games.Contains(game.GameId)).ToEnumerable(),
            "User.Games");
    }

    public async Task<GameModel> GetGame(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(game => game.GameId == id).SingleOrDefault(),
            "GameId");
    }

    public async Task<string> GetGameNickname(Guid gameId)
    {
        var game = await GetGame(gameId);
        return game.Nickname;
    }

    public async Task<IEnumerable<MinifiedGameModel>> GetMostRecent20Games(UserModel user)
    {
        var games = await ThrowIfNull(
            user.Games,
            games => Collection.Find(x => !games.Contains(x.GameId)).Limit(20).ToEnumerable(),
            "User.Games");

        return await Task.WhenAll(games.Select(async game => await MinifiedGameModel.ParseFromModel(game, _trainerService)));
    }

    public async Task<bool> HasGM(Guid gameId)
    {
        var trainers = await _trainerService.GetTrainersByGameId(gameId);
        var isGm = trainers.Any(trainer => trainer.IsGM);
        return await Task.FromResult(isGm);
    }

    public async Task PostGame(GameModel game)
    {
        await PostDocument(game);
    }

    public async Task<GameModel> UpdateGameLogs(GameModel theGame, params LogModel[] logs)
    {
        return await UpdateDocument(
            theGame.GameId,
            game => game.GameId == theGame.GameId,
            Builders<GameModel>.Update.Set("Logs", theGame.Logs?.Union(logs) ?? logs));
    }

    public async Task<GameModel> UpdateGameNpcList(Guid gameId, IEnumerable<Guid> npcIds)
    {
        return await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            Builders<GameModel>.Update.Set("NPCs", npcIds));
    }

    public async Task<GameModel> UpdateGameOnlineStatus(Guid gameId, bool isOnline)
    {
        return await UpdateDocument(
            gameId,
            game => game.GameId == gameId,
            Builders<GameModel>.Update.Set("IsOnline", isOnline));
    }
}
