using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/game")]
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
    ILogger<GameController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<GameController> _logger = logger;
    private readonly ISpriteService _spriteService = spriteService;
    private readonly IExportService _exportService = exportService;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly INpcService _npcService = npcService;

    [HttpGet("user/{userId}", Name = nameof(GetAllGames))]
    [ProducesResponseType(typeof(IEnumerable<FoundGameResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<MinifiedGameModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllGames(
        Guid userId,
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromQuery] string nickname)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            var games = await GameService.GetAllGames(nickname);
            var responses = await Task.WhenAll(games
                .Where(game => !user.Games.Contains(game.GameId))
                .Select(async game =>
                {
                    var trainers = await TrainerService.GetTrainersByGameId(game.GameId);
                    return await FoundGameResponse.ParseFromModel(trainers, game.GameId, PokemonService, PokedexService, game.Nickname);
                }));
            return Ok(responses);
        }

        var recentGames = await GameService.GetMostRecent20Games(user);
        return Ok(recentGames);
    }

    [HttpGet("user/games/{userId}", Name = nameof(GetAllUserGames))]
    [ProducesResponseType(typeof(BasePokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllUserGames(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        var games = await GameService.GetAllGamesWithUser(user);
        var responses = await Task.WhenAll(games.Select(async game =>
        {
            var trainers = await TrainerService.GetTrainersByGameId(game.GameId);
            return await FoundGameResponse.ParseFromModel(trainers, game.GameId, PokemonService, PokedexService, game.Nickname);
        }));
        return Ok(responses);
    }

    [HttpGet("sprites/all", Name = nameof(GetAllSprites))]
    [ProducesResponseType(typeof(IEnumerable<SpriteModel>), 200)]
    public async Task<IActionResult> GetAllSprites()
    {
        var sprites = await _spriteService.GetAllSprites();
        return Ok(sprites.OrderBy(sprite => sprite.FriendlyText));
    }

    [HttpGet("getGame/{gameId}", Name = nameof(GetGame))]
    [ProducesResponseType(typeof(FoundGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetGame(Guid gameId)
    {
        var game = await GameService.GetGame(gameId);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        return Ok(await FoundGameResponse.ParseFromModel(trainers, gameId, PokemonService, PokedexService, game.Nickname));
    }

    [HttpGet("{gameId}/trainer/{trainerId}", Name = nameof(GetTrainerInGame))]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerInGame(Guid gameId, Guid trainerId)
    {
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }
    [HttpGet("{gameId}/all_logs")]
    [ProducesResponseType(typeof(AllLogsResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetAllLogs(Guid gameId)
    {
        var game = await GameService.GetGame(gameId);
        return Ok(new AllLogsResponse(game));
    }

    [HttpGet("{gameId}/logs")]
    [ProducesResponseType(typeof(ICollection<LogModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetLogs(Guid gameId)
    {
        var game = await GameService.GetGame(gameId);
        if (game.Logs == null)
        {
            return Ok(Array.Empty<LogModel>());
        }

        return Ok(game.Logs.OrderByDescending(log => log.LogTimestamp).Take(50).ToArray());
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ImportGame()
    {
        var json = Request.GetJsonFromRequest();
        if (string.IsNullOrEmpty(json))
        {
            return BadRequest(new GenericResponse("empty json file"));
        }
        await _exportService.TryParseImport(json);
        return Ok();
    }

    [HttpPost("{userId}/newGame")]
    [ProducesResponseType(typeof(CreatedGameResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> CreateNewGame(
        Guid userId,
        [FromQuery] string nickname,
        [FromQuery] string gameSessionPassword,
        [FromQuery] string username)
    {
        var game = await BuildGame(nickname, gameSessionPassword);
        await GameService.PostGame(game);

        var gm = await BuildGM(game.GameId, userId, username);

        await TrainerService.PostTrainer(gm);
        var gameCreationLog = new LogModel
        (
            user: gm.TrainerName,
            action: "created a new game and joined as game master"
        );
        await GameService.UpdateGameLogs(game, gameCreationLog);
        await RefreshToken(userId);
        return Ok(await CreatedGameResponse.ParseFromModel(gm, PokemonService, PokedexService));
    }

    [HttpPost("{gameId}/{userId}/newUser")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> AddPlayerToGame(
        Guid gameId,
        Guid userId,
        [FromQuery] string username)
    {
        var game = await GameService.GetGame(gameId);
        if (!await GameService.HasGM(gameId))
        {
            return BadRequest();
        }

        var trainer = await BuildTrainer(gameId, userId, username);
        await TrainerService.PostTrainer(trainer);

        var trainerCreationLog = new LogModel
        (
            user: trainer.TrainerName,
            action: "joined"
        );
        await GameService.UpdateGameLogs(game, trainerCreationLog);
        await RefreshToken(userId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPost("{gameId}/{trainerId}/log")]
    [ProducesResponseType(typeof(LogModel), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> PostLogAsync(
        [FromBody] LogModel log,
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var game = await GameService.GetGame(gameId);
        log.Action += $" at {DateTime.UtcNow}";
        await GameService.UpdateGameLogs(game, log);
        await RefreshToken(trainerId);
        return Ok(log);
    }

    [HttpPost("{gameId}/{gameMasterId}/{trainerId}/allow")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AllowUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        trainer.IsAllowed = true;
        await TrainerService.UpdateTrainer(trainer);
        var log = new LogModel
        (
            user: trainer.TrainerName,
            action: "joined the game"
        );
        await GameService.UpdateGameLogs(game, log);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPost("{gameId}/{gameMasterId}/{trainerId}/disallow")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DisallowUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        trainer.IsAllowed = false;
        await TrainerService.UpdateTrainer(trainer);
        var log = new LogModel
        (
            user: trainer.TrainerName,
            action: "was removed from the game"
        );
        await GameService.UpdateGameLogs(game, log);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPatch("{gameId}/{trainerId}/addStats")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddTrainerStats(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] Trainer trainerDto,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        await CompleteTrainer(trainerId, gameId, trainerDto);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPatch("{gameId}/{gameMasterId}/start")]
    [ProducesResponseType(typeof(FoundGameResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> StartGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] string gameSessionPassword)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await IsGameAuthenticated(gameSessionPassword, game);
        await AssignAuthAndToken(trainer.TrainerId);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        return Ok(await FoundGameResponse.ParseFromModel(trainers, gameId, PokemonService, PokedexService, game.Nickname));
    }

    [HttpPatch("{gameId}/{gameMasterId}/end")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EndGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await SetEndGameStatuses(gameId);
        return Ok();
    }

    [HttpPatch("{gameId}/{gameMasterId}/addNpcs")]
    [ProducesResponseType(typeof(UpdatedNpcListResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddNPCsToGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] Guid[] npcIds)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId);
        var newNpcList = game.NPCs.Union(foundNpcIds);
        await RefreshToken(gameMasterId);
        return await UpdateNpcList(gameId, newNpcList);
    }

    [HttpPatch("{gameId}/{gameMasterId}/removeNpcs")]
    [ProducesResponseType(typeof(UpdatedNpcListResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemovesNPCsFromGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] Guid[] npcIds)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var foundNpcIds = await GetNpcs(npcIds);
        var game = await GameService.GetGame(gameId);
        var newNpcList = game.NPCs.Except(foundNpcIds);
        await RefreshToken(gameMasterId);
        return await UpdateNpcList(gameId, newNpcList);
    }

    [HttpDelete("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
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


    [HttpDelete("{gameId}/{gameMasterId}/export")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ExportGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] string gameSessionPassword)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var gameMaster = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await _encryptionService.VerifySecret(gameSessionPassword, game.PasswordHash);

        var exportLog = new LogModel
        (
            user: gameMaster.TrainerName,
            action: "exported game session"
        );
        await GameService.UpdateGameOnlineStatus(gameId, false);
        await GameService.UpdateGameLogs(game, exportLog);
        var exportStream = await _exportService.GetExportStream(game);

        await DeleteGame(accessToken, sessionAuth, gameId, gameMasterId, gameSessionPassword);
        return File(
            exportStream,
            "application/octet-stream",
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
        gm.Sprite = "acetrainer";
        return gm;
    }

    private async Task<TrainerModel> BuildTrainer(
        Guid gameId,
        Guid userId,
        string username)
    {
        var trainer = await BuildTrainer(
            gameId,
            userId,
            username,
            false);

        trainer.Sprite = "acetrainer";
        return trainer;
    }

    private async Task<TrainerModel> BuildTrainer(
        Guid gameId,
        Guid userId,
        string username,
        bool isGM)
    {
        if (await TrainerService.GetTrainerByUsername(username, gameId) != null)
        {
            throw new DuplicateEntryException(typeof(TrainerModel));
        }

        var trainer = await CreateTrainer(gameId, userId, username);
        trainer.IsGM = isGM;
        trainer.IsAllowed = isGM;
        return trainer;
    }

    private async Task<TrainerModel> CreateTrainer(
        Guid gameId,
        Guid userId,
        string username)
    {
        var user = await UserService.GetUserById(userId);
        user.Games.Add(gameId);
        await UserService.UpdateUser(user);
        return new TrainerModel
        {
            GameId = gameId,
            TrainerId = userId,
            Honors = [],
            TrainerName = username,
            TrainerClasses = [],
            Feats = [],
            IsOnline = true,
            Items = [],
            TrainerStats = new StatsModel
            {
                HP = 20,
                Attack = 1,
                Defense = 1,
                SpecialAttack = 1,
                SpecialDefense = 1,
                Speed = 1
            },
            CurrentHP = 20,
            Origin = string.Empty
        };
    }

    private async Task<IEnumerable<Guid>> GetNpcs(Guid[] npcIds)
    {
        var npcs = await Task.WhenAll(npcIds.Select(_npcService.GetNpc));
        var foundNpcs = npcs.Select(x => x.NPCId);
        return foundNpcs;
    }

    private async Task<OkObjectResult> UpdateNpcList(
        Guid gameId,
        IEnumerable<Guid> newNpcList)
    {
        await GameService.UpdateGameNpcList(gameId, newNpcList);
        return Ok(new UpdatedNpcListResponse(newNpcList));
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

    private async Task CompleteTrainer(
        Guid trainerId,
        Guid gameId,
        Trainer trainerDto)
    {
        var user = await UserService.GetUserById(trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (CheckRequestingTrainer(trainerDto, trainer, user))
        {
            await AddTrainerPokemon(trainerDto.NewPokemon, trainer);
            await SetStartingEquipmentWithOrigin(trainer);
            await TrainerService.UpdateTrainer(trainer);
            var game = await GameService.GetGame(trainer.GameId);
            var statsAddedLog = new LogModel(trainer.TrainerName, $"has updated stats");
            await GameService.UpdateGameLogs(game, statsAddedLog);
        }
    }

    private async Task AddTrainerPokemon(
        IEnumerable<NewPokemon> pokemon,
        TrainerModel trainer)
    {
        foreach (var data in pokemon.Where(data => data != null))
        {
            var nickname = data.Nickname.Length > 18 ? data.Nickname[..18] : data.Nickname;
            var pokemonModel = await DexService.GetNewPokemon(data.SpeciesName, nickname, data.Form);
            pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
            pokemonModel.OriginalTrainerId = trainer.TrainerId;
            pokemonModel.TrainerId = trainer.TrainerId;
            pokemonModel.GameId = trainer.GameId;
            pokemonModel.Pokeball = Pokeball.Basic_Ball.ToString().Replace("_", "");
            await PokemonService.PostPokemon(pokemonModel);
            var game = await GameService.GetGame(trainer.GameId);
            var caughtPokemonLog = new LogModel(trainer.TrainerName, $"caught a {pokemonModel.SpeciesName} named {pokemonModel.Nickname}");
            await GameService.UpdateGameLogs(game, caughtPokemonLog);
            if (await PokedexService.GetPokedexItem(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo) == null)
            {
                await PokedexService.PostDexItem(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo, true, true);
            }
            else
            {
                await PokedexService.UpdateDexItemIsCaught(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo);
            }
        }
    }

    private async Task SetStartingEquipmentWithOrigin(TrainerModel trainer)
    {
        var origin = await DexService.GetDexEntry<OriginModel>(DexType.Origins, trainer.Origin);
        var collection = await Task.WhenAll(origin.StartingEquipmentList.Select(ConvertStartingEquipment));
        trainer.Items = [.. collection];
    }

    private async Task<ItemModel> ConvertStartingEquipment(StartingEquipment s)
    {
        BaseItemModel baseItem = s.Type switch
        {
            StartingEquipmentType.Trainer => await  DexService.GetDexEntry<BaseItemModel>(DexType.TrainerEquipment, s.Name),
            StartingEquipmentType.Pokeball => await DexService.GetDexEntry<BaseItemModel>(DexType.Pokeballs, s.Name),
            StartingEquipmentType.Medical => await DexService.GetDexEntry<BaseItemModel>(DexType.MedicalItems, s.Name),
            StartingEquipmentType.Berry => await DexService.GetDexEntry<BaseItemModel>(DexType.Berries, s.Name),
            StartingEquipmentType.Pokemon => await DexService.GetDexEntry<BaseItemModel>(DexType.PokemonItems, s.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(s.Type)),
        };
        var item = new ItemModel
        {
            Name = baseItem.Name,
            Effects = baseItem.Effects,
            Amount = s.Amount,
            Type = s.Type.ToString()
        };
        return item;
    }

    private static bool CheckRequestingTrainer(
        Trainer trainerDto,
        TrainerModel requestingTrainer,
        UserModel user)
    {
        if (user.SiteRole == UserRoleOnSite.SiteAdmin)
        {
            return true;
        }

        if (trainerDto.GameId != requestingTrainer.GameId)
        {
            return false;
        }

        return trainerDto.TrainerId == requestingTrainer.TrainerId || requestingTrainer.IsGM;
    }
}
