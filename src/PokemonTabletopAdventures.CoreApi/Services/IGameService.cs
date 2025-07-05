using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IGameService
{
    /// <summary>
    /// Returns a game matching the game session id
    /// </summary>
    /// <param name="id">The game session id</param>
    public Task<GameModel> GetGame(Guid id);

    /// <summary>
    /// Returns all games that contains the supplied nickname as a substring
    /// </summary>
    /// <param name="nickname">The nickname to search with</param>
    public Task<IEnumerable<GameModel>> GetAllGames(string nickname);

    /// <summary>
    /// Returns all games that the user is a part of
    /// </summary>
    /// <param name="user">The user to search with</param>
    public Task<IEnumerable<GameModel>> GetAllGamesWithUser(UserModel user);

    /// <summary>
    /// Returns all games in db
    /// </summary>
    public Task<IEnumerable<MinifiedGameModel>> GetMostRecent20Games(UserModel user);

    /// <summary>
    /// Returns a game's nickname using the game id
    /// </summary>
    /// <param name="gameId">The game session id</param>
    public Task<string> GetGameNickname(Guid gameId);

    /// <summary>
    /// Attempts to add a game using the provided document
    /// </summary>
    /// <param name="game">The document to add</param>
    public Task PostGame(GameModel game);

    /// <summary>
    /// Searches for a game, then updates the npc list
    /// </summary>
    /// <param name="gameId">The game session id</param>
    /// <param name="npcIds">The updated npc list</param>
    public Task<GameModel> UpdateGameNpcList(Guid gameId, IEnumerable<Guid> npcIds);

    /// <summary>
    /// Searches for a game, then updates its online status
    /// </summary>
    /// <param name="gameId">The game session id</param>
    /// <param name="isOnline">The updated online status</param>
    public Task<GameModel> UpdateGameOnlineStatus(
        Guid gameId,
        bool isOnline);

    /// <summary>
    /// Searches for a game, then updates the logs
    /// </summary>
    /// <param name="theGame">The game session</param>
    /// <param name="logs">The new logs to add</param>
    public Task<GameModel> UpdateGameLogs(GameModel theGame, params LogModel[] logs);

    /// <summary>
    /// Searches for a game using its id, then deletes it
    /// </summary>
    /// <param name="id">The game session id</param>
    public Task DeleteGame(Guid id);

    /// <summary>
    /// Returns whether there is a game master for the provide game session
    /// </summary>
    /// <param name="gameId">The game session id</param>
    /// <param name="error">The error</param>
    public Task<bool> HasGM(Guid gameId);
}
