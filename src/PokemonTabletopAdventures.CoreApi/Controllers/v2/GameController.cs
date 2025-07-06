using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Games;
using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.DTOs.Settings;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.GameRoute)]
public class GameController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    INpcService npcService,
    IGameService gameService,
    IDexService dexUtility,
    ISpriteService spriteService,
    IPokedexService pokedexService,
    IExportService exportService,
    IEncryptionService encryptionService,
    ISettingService settingService,
    IShopService shopService,
    ILogger<GameController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<GameController> _logger = logger;
    private readonly ISpriteService _spriteService = spriteService;
    private readonly IExportService _exportService = exportService;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly INpcService _npcService = npcService;
    private readonly ISettingService _settingService = settingService;
    private readonly IShopService _shopService = shopService;

    [HttpGet("retrieve", Name = nameof(GetAllGames))]
    [ProducesResponseType(typeof(IEnumerable<RetrieveGameResponse>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllGames(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromQuery] Guid userId,
        [FromQuery] string nickname)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        IEnumerable<GameModel> gameModels;
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            gameModels = await GameService.GetAllGames(nickname);
        }
        else
        {
            gameModels = await GameService.GetMostRecent20Games(user);
        }

        var games = await ParseFromModel(
            gameModels,
            false,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new RetrieveGameResponse { Games = games });
    }

    [HttpGet("user/{userId}/retrieve", Name = nameof(GetAllUserGames))]
    [ProducesResponseType(typeof(RetrieveGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllUserGames(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        var gameModels = await GameService.GetAllGamesWithUser(user);
        var games = await ParseFromModel(
            gameModels,
            false,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new RetrieveGameResponse { Games = games });
    }

    [HttpGet("sprites/retrieve", Name = nameof(GetAllSprites))]
    [ProducesResponseType(typeof(IEnumerable<SpriteModel>), 200)]
    public async Task<IActionResult> GetAllSprites()
    {
        var sprites = await _spriteService.GetAllSprites();
        return Ok(sprites.OrderBy(sprite => sprite.FriendlyText));
    }

    [HttpGet("{gameId}/retrieve", Name = nameof(GetGame))]
    [ProducesResponseType(typeof(RetrieveGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetGame(Guid gameId)
    {
        var game = await GameService.GetGame(gameId);
        var games = await ParseFromModel(
            [game],
            false,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new RetrieveGameResponse { Games = games });
    }

    [HttpGet("{gameId}/logs/retrieve", Name = nameof(GetLogs))]
    [ProducesResponseType(typeof(RetrieveLogsResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int count,
        Guid gameId)
    {
        var game = await GameService.GetGame(gameId);
        return Ok(new RetrieveLogsResponse(game, count));
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(CreateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ImportGame()
    {
        var json = Request.GetJsonFromRequest();
        if (string.IsNullOrEmpty(json))
        {
            throw new ImportFailedException(Constants.PtaExceptionParts.EmptyImportJsonMessage);
        }
        var game = await _exportService.ParseImport(json);
        var games = await ParseFromModel(
            [game],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new CreateGameResponse { Games = games });
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateGameResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> CreateNewGame(
        [FromBody] CreateGameRequest request)
    {
        var game = await BuildGame(request.GameNickname, request.GameSessionPassword);
        await GameService.PostGame(game);

        var gm = await BuildGM(game.GameId, request.UserId, request.Username);

        await TrainerService.PostTrainer(gm);
        var gameCreationLog = new LogModel
        (
            user: gm.TrainerName,
            action: GameLogMessages.GameCreationLog
        );
        await GameService.UpdateGameLogs(game, gameCreationLog);
        await RefreshToken(request.UserId);
        var games = await ParseFromModel(
            [game],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new CreateGameResponse { Games = games });
    }

    [HttpPatch("{gameId}/logs/add")]
    [ProducesResponseType(typeof(UpdateGameResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddLogsAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.UserId);
        var game = await GameService.GetGame(gameId);
        var logs = request.Game.Logs;
        foreach (var log in logs)
        {
            log.LogTimestamp = DateTimeOffset.Now;
        }
        await GameService.UpdateGameLogs(game, [.. logs]);
        await RefreshToken(request.UserId);
        var games = await ParseFromModel(
            [game],
            false,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new UpdateGameResponse { Games = games });
    }

    [HttpPatch("{gameId}/start")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> StartGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await IsUserGM(request.GameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(request.GameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await IsGameAuthenticated(request.GameSessionPassword!, game);
        await GameService.UpdateGameOnlineStatus(gameId, true);
        await AssignAuthAndToken(trainer.TrainerId);
        var games = await ParseFromModel(
            [game],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new UpdateGameResponse { Games = games });
    }

    [HttpPatch("{gameId}/end")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EndGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await IsUserGM(request.GameMasterId, gameId, accessToken, sessionAuth);
        await SetEndGameStatuses(gameId);
        var game = await GameService.GetGame(gameId);
        var games = await ParseFromModel(
            [game],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new UpdateGameResponse { Games = games });
    }

    [HttpPatch("{gameId}/npcs/add")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddNPCsToGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        var npcIds = request.Game.Npcs.Select(npc => npc.NpcId);
        await IsUserGM(request.GameMasterId, gameId, accessToken, sessionAuth);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId);
        var newNpcList = game.NPCs.Union(foundNpcIds);
        await RefreshToken(request.GameMasterId);
        var updatedGame = await GameService.UpdateGameNpcList(gameId, newNpcList);
        var games = await ParseFromModel(
            [updatedGame],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new UpdateGameResponse { Games = games });
    }

    [HttpPatch("{gameId}/npcs/remove")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemovesNPCsFromGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        var npcIds = request.Game.Npcs.Select(npc => npc.NpcId);
        await IsUserGM(request.GameMasterId, gameId, accessToken, sessionAuth);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId);
        var newNpcList = game.NPCs.Except(foundNpcIds);
        await RefreshToken(request.GameMasterId);
        var updatedGame = await GameService.UpdateGameNpcList(gameId, newNpcList);
        var games = await ParseFromModel(
            [updatedGame],
            true,
            _npcService,
            _settingService,
            _shopService);
        return Ok(new UpdateGameResponse { Games = games });
    }

    [HttpDelete("{gameId}/delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        [FromQuery] Guid gameMasterId,
        [FromQuery] string gameSessionPassword)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        await IsGameAuthenticated(gameSessionPassword, game);
        await MassDeletePokemon(gameId);
        await TrainerService.DeleteTrainersByGameId(gameId);
        await GetGameDeletion(gameId);
        return Ok();
    }


    [HttpDelete("{gameId}/export")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ExportGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        [FromQuery] Guid gameMasterId,
        [FromQuery] string gameSessionPassword)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var gameMaster = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await _encryptionService.VerifySecret(gameSessionPassword, game.PasswordHash);

        var exportLog = new LogModel(
            user: gameMaster.TrainerName,
            action: GameLogMessages.ExportGameLog);
        await GameService.UpdateGameOnlineStatus(gameId, false);
        await GameService.UpdateGameLogs(game, exportLog);
        var exportStream = await _exportService.GetExportStream(game);

        await DeleteGame(accessToken, sessionAuth, gameId, gameMasterId, gameSessionPassword);
        return File(
            exportStream,
            ContentTypes.OctetStream,
            $"{game.Nickname}.json");
    }
    
    private async Task<GameModel> BuildGame(string nickname, string gameSessionPassword)
    {
        var guid = Guid.NewGuid();
        return new GameModel
        {
            GameId = guid,
            Nickname = string.IsNullOrEmpty(nickname)
                ? guid.ToString().Split('-')[0]
                : nickname,
            IsOnline = true,
            PasswordHash = await _encryptionService.HashSecret(gameSessionPassword),
            NPCs = [],
            Logs = []
        };
    }

    private async Task<TrainerModel> BuildGM(
        Guid gameId,
        Guid userId,
        string username)
    {
        var gm = await BuildTrainer(
            gameId,
            userId,
            username,
            true);

        gm.IsGM = true;
        gm.Sprite = Sprites.AceTrainer;
        return gm;
    }

    private async Task<IEnumerable<Guid>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var npcs = await Task.WhenAll(npcIds.Select(_npcService.GetNpc));
        var foundNpcs = npcs.Select(x => x.NPCId);
        return foundNpcs;
    }

    private async Task SetEndGameStatuses(Guid gameId)
    {
        await GameService.UpdateGameOnlineStatus(
            gameId,
            false);

        foreach (var trainer in await TrainerService.GetTrainersByGameId(gameId))
        {
            await TrainerService.UpdateTrainerOnlineStatus(
                trainer.TrainerId,
                trainer.GameId,
                false);
        }
    }

    private async Task GetGameDeletion(Guid gameId)
    {
        await GameService.DeleteGame(gameId);
    }

    private async Task MassDeletePokemon(Guid gameId)
    {
        foreach (var trainer in await TrainerService.GetTrainersByGameId(gameId))
        {
            await PokemonService.DeletePokemonByTrainerId(gameId, trainer.TrainerId);
        }
    }
}
