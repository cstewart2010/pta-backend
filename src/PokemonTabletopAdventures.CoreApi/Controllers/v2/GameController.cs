using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Trainers;

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
    IEncryptionService encryptionService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<GameController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    private readonly ILogger<GameController> _logger = logger;
    private readonly IEncryptionService _encryptionService = encryptionService;

    [HttpGet("retrieve", Name = nameof(GetAllGames))]
    [ProducesResponseType(typeof(IEnumerable<RetrieveGameResponse>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllGames(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromQuery] Guid userId,
        [FromQuery] string nickname)
    {
        await VerifyIdentity(sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        IEnumerable<Game> gameModels;
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            gameModels = await GameService.GetAllGames(nickname);
        }
        else
        {
            gameModels = await GameService.GetMostRecent20Games(user);
        }

        return Ok(new RetrieveGameResponse { Games = [.. gameModels] });
    }

    [HttpGet("user/{userId}/retrieve", Name = nameof(GetAllUserGames))]
    [ProducesResponseType(typeof(RetrieveGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllUserGames(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId)
    {
        await VerifyIdentity(sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        var gameModels = await GameService.GetAllGamesWithUser(user);
        return Ok(new RetrieveGameResponse { Games = [.. gameModels] });
    }

    [HttpGet("sprites/retrieve", Name = nameof(GetAllSprites))]
    [ProducesResponseType(typeof(IEnumerable<SpriteDto>), 200)]
    public async Task<IActionResult> GetAllSprites()
    {
        var sprites = await spriteService.GetAllSprites();
        return Ok(sprites.OrderBy(sprite => sprite.FriendlyText));
    }

    [HttpGet("{gameId}/retrieve", Name = nameof(GetGame))]
    [ProducesResponseType(typeof(RetrieveGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetGame(Guid gameId)
    {
        var game = await GameService.GetGame(gameId, false);
        return Ok(new RetrieveGameResponse { Games = [game] });
    }

    [HttpGet("{gameId}/logs/retrieve", Name = nameof(GetLogs))]
    [ProducesResponseType(typeof(RetrieveLogsResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int count,
        Guid gameId)
    {
        var game = await GameService.GetGame(gameId, false);
        return Ok(CreateRetrieveLogsResponse(game, count));
    }

    [HttpGet("refresh")]
    [ProducesResponseType(typeof(RetrieveGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RefreshInGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId,
        Guid gameId,
        [FromQuery] bool isGM)
    {
        if (isGM)
        {
            return await GetUpdatedGM(sessionAuth, userId, gameId);
        }

        return await GetUpdatedTrainer(sessionAuth, userId, gameId);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateGameResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> CreateNewGame(
        [FromBody] CreateGameRequest request)
    {
        var game = BuildGame(request.GameNickname);
        var passwordHash = await _encryptionService.HashSecret(request.GameSessionPassword);
        await GameService.PostGame(game, passwordHash);

        var gm = await BuildGM(game.GameId, request.UserId, request.Username);

        await TrainerService.PostTrainer(gm);
        var gameCreationLog = new Log
        {
            User = gm.TrainerName,
            Action = GameLogMessages.GameCreationLog,
            LogTimestamp = DateTimeOffset.Now
        };
        var updatedGame = await GameService.UpdateGameLogs(game, true, gameCreationLog);
        return Ok(new CreateGameResponse { Games = [updatedGame] });
    }

    [HttpPatch("{gameId}/logs/add")]
    [ProducesResponseType(typeof(UpdateGameResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddLogsAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        var isGm = await VerifyIdentity(sessionAuth, request.UserId, gameId);
        var game = await GameService.GetGame(gameId, isGm);
        await GameService.UpdateGameLogs(game, isGm, [.. request.Game.Logs]);
        return Ok(new UpdateGameResponse { Games = [game] });
    }

    [HttpPatch("{gameId}/start")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> StartGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await IsUserGM(request.GameMasterId, gameId, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(request.GameMasterId, gameId);
        var game = await GameService.GetGame(gameId, true);
        await IsGameAuthenticated(request.GameSessionPassword!, game);
        await GameService.UpdateGameOnlineStatus(gameId, true);
        await AssignAuthAndToken(trainer.TrainerId);
        return Ok(new UpdateGameResponse { Games = [game] });
    }

    [HttpPatch("{gameId}/end")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EndGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await IsUserGM(request.GameMasterId, gameId, sessionAuth);
        await SetEndGameStatuses(gameId);
        var game = await GameService.GetGame(gameId, true);
        return Ok(new UpdateGameResponse { Games = [game] });
    }

    [HttpPatch("{gameId}/npcs/add")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddNPCsToGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        await IsUserGM(request.GameMasterId, gameId, sessionAuth);
        var npcIds = request.Game.Npcs.Select(npc => npc.NpcId);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId, true);
        var newNpcList = game.Npcs.Select(x => x.NpcId).Union(foundNpcIds);
        var updatedGame = await GameService.UpdateGameNpcList(gameId, [..newNpcList]);
        return Ok(new UpdateGameResponse { Games = [updatedGame] });
    }

    [HttpPatch("{gameId}/npcs/remove")]
    [ProducesResponseType(typeof(UpdateGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemovesNPCsFromGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateGameRequest request,
        Guid gameId)
    {
        var npcIds = request.Game.Npcs.Select(npc => npc.NpcId);
        await IsUserGM(request.GameMasterId, gameId, sessionAuth);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId, true);
        var newNpcList = game.Npcs.Select(x => x.NpcId).Except(foundNpcIds);
        var updatedGame = await GameService.UpdateGameNpcList(gameId, [.. newNpcList]);
        return Ok(new UpdateGameResponse { Games = [updatedGame] });
    }

    [HttpDelete("{gameId}/delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteGame(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        [FromQuery] Guid gameMasterId,
        [FromQuery] string gameSessionPassword)
    {
        await IsUserGM(gameMasterId, gameId, sessionAuth);
        var game = await GameService.GetGame(gameId, true);
        await IsGameAuthenticated(gameSessionPassword, game);
        await MassDeletePokemon(gameId);
        await TrainerService.DeleteTrainer(gameMasterId, gameId);
        await GetGameDeletion(gameId);
        return Ok();
    }
    
    private static Game BuildGame(string nickname)
    {
        var guid = Guid.NewGuid();
        return new Game
        {
            GameId = guid,
            Nickname = string.IsNullOrEmpty(nickname)
                ? guid.ToString().Split('-')[0]
                : nickname,
            IsOnline = true,
            Npcs = [],
            Logs = [],
            Settings = [],
            Trainers = [],
        };
    }

    private async Task<Trainer> BuildGM(
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

    private async Task<OkObjectResult> GetUpdatedTrainer(
        string sessionAuth,
        Guid userId,
        Guid gameId)
    {
        await VerifyIdentity(sessionAuth, userId);
        var game = await GameService.GetGame(gameId, false);
        return Ok(new RetrieveGameResponse { Games = [game] });
    }

    private async Task<OkObjectResult> GetUpdatedGM(
        string sessionAuth,
        Guid userId,
        Guid gameId)
    {
        await IsUserGM(userId, gameId, sessionAuth);
        var game = await GameService.GetGame(gameId, true);
        return Ok(new RetrieveGameResponse { Games = [game] });
    }

    private async Task<IEnumerable<Guid>> GetNpcs(IEnumerable<Guid> npcIds)
    {
        var npcs = await Task.WhenAll(npcIds.Select(npcService.GetNpc));
        var foundNpcs = npcs.Select(x => x.NpcId);
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
