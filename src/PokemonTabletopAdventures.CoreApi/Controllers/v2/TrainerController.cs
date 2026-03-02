using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.TrainerRoute)]
public class TrainerController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<TrainerController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    private readonly ILogger<TrainerController> _logger = logger;

    [HttpGet("retrieve/all")]
    [ProducesResponseType(typeof(RetrieveTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainers(
        [FromBody] RetrieveTrainerRequest request)
    {
        var trainers = await GetTrainers(request.GameId);
        return Ok(new RetrieveTrainerResponse { Trainers = trainers});
    }

    [HttpPost("retrieve/name")]
    [ProducesResponseType(typeof(RetrieveTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetTrainer(
        [FromBody] RetrieveTrainerRequest request)
    {
        var trainer = await TrainerService.GetTrainerByUsername(request.TrainerName!, request.GameId) ?? throw new UnknownEntityException<Trainer>(nameof(request.TrainerName), request.TrainerName);
        return Ok(new RetrieveTrainerResponse { Trainers = [trainer] });
    }

    [HttpPost("retrieve/id", Name = nameof(GetTrainerInGame))]
    [ProducesResponseType(typeof(RetrieveTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerInGame(
        [FromBody] RetrieveTrainerRequest request)
    {
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        return Ok(new RetrieveTrainerResponse { Trainers = [trainer] });
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> PostNewPlayer(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreateTrainerRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        request.Trainer.GameId = request.GameId;
        request.Trainer.Level = 1;
        request.Trainer.Origin = string.Empty;
        request.Trainer.TrainerClasses = [];
        request.Trainer.PokeDex = [];
        request.Trainer.PokemonHome = [];
        request.Trainer.NewPokemon = [];
        request.Trainer.PokemonTeam = [];
        request.Trainer.Items = [];
        request.Trainer.TrainerSkills = [];
        request.Trainer.TrainerStats = new();
        request.Trainer.CurrentHP = request.Trainer.TrainerStats.HP;
        request.Trainer.IsAllowed = false;
        request.Trainer.IsComplete = false;
        request.Trainer.CaughtTotal = 0;
        request.Trainer.SeenTotal = 0;
        request.Trainer.Money = 0;
        request.Trainer.IsGM = false;
        request.Trainer.TrainerId = request.UserId;
        await TrainerService.PostTrainer(request.Trainer);
        var trainer = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        return Ok(new CreateTrainerResponse { Trainer = trainer });
    }

    [HttpPut("allow")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AllowUser(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        var game = await GameService.GetGame(request.GameId, true);
        trainer.IsAllowed = true;
        var updatedTrainer = await TrainerService.UpdateTrainer(trainer);
        var log = new Log
        {
            User = trainer.TrainerName,
            Action = GameLogMessages.JoinedTheGameLog,
            LogTimestamp = DateTimeOffset.UtcNow,
        };
        await GameService.UpdateGameLogs(game, true, log);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("disallow")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DisallowUser(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        var game = await GameService.GetGame(request.GameId, true);
        trainer.IsAllowed = false;
        var updatedTrainer = await TrainerService.UpdateTrainer(trainer);
        var log = new Log
        {
            User = trainer.TrainerName,
            Action = GameLogMessages.JoinedTheGameLog,
            LogTimestamp = DateTimeOffset.UtcNow,
        };
        await GameService.UpdateGameLogs(game, true, log);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("update/stats")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddTrainerStats(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        var trainer = request.Trainers.SingleOrDefault() ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var updatedTrainer = await CompleteTrainer(request.UserId, request.GameId, trainer);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("update/honor/all")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddGroupHonor(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        if (string.IsNullOrEmpty(request.Honor))
        {
            throw new InvalidTrainerException("No honor was listed");
        }
        var trainers = await TrainerService.GetTrainersByGameId(request.GameId);
        foreach (var trainer in trainers)
        {
            await TrainerService.UpdateTrainerHonors(trainer.TrainerId, trainer.GameId, trainer.Honors.Append(request.Honor));
        }

        var updatedTrainers = await Task.WhenAll(trainers.Select(async trainer => await TrainerService.UpdateTrainerHonors(trainer.TrainerId, request.GameId, trainer.Honors.Append(request.Honor!))));
        var updatedHonorsLog = new Log
        {
            User = GameLogMessages.PartyUser,
            Action = $"has earned a new honor: {request.Honor}",
            LogTimestamp = DateTimeOffset.Now
        };

        await GameService.UpdateGameLogs(game, true, updatedHonorsLog);
        return Ok(new UpdateTrainerResponse { Trainers = updatedTrainers });
    }

    [HttpPut("update/honor")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddSingleHonor(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainerId = request.Trainers.SingleOrDefault()?.TrainerId ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var trainer = await TrainerService.GetTrainerById(trainerId, request.GameId);
        var gm = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        var game = await GameService.GetGame(request.GameId, true);
        var updatedTrainer = await TrainerService.UpdateTrainerHonors(trainerId, request.GameId, trainer.Honors.Append(request.Honor!));
        var updatedHonorsLog = new Log
        {
            User = gm.TrainerName,
            Action = $"has granted {trainer.TrainerName} a new honor",
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, true, updatedHonorsLog);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("add/items")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToTrainerAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainerId = request.Trainers.SingleOrDefault()?.TrainerId ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var items = request.Trainers.SingleOrDefault()?.Items ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var trainer = await TrainerService.GetTrainerById(trainerId, request.GameId);
        var game = await GameService.GetGame(request.GameId, true);
        var addedItemsLogs = await AddItemsToTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, true, [.. addedItemsLogs]);
        var updatedTrainer = await TrainerService.GetTrainerById(trainerId, request.GameId);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("add/items/all")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToAllTrainersAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(request.GameId);
        var game = await GameService.GetGame(request.GameId, true);
        var items = request.Trainers.SingleOrDefault()?.Items ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        foreach (var trainer in trainers)
        {
            var addedItemsLogs = await AddItemsToTrainer(trainer, items);
            await GameService.UpdateGameLogs(game, true, [.. addedItemsLogs]);
        }

        return Ok();
    }

    [HttpPut("remove/items")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        var trainer = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        var game = await GameService.GetGame(request.GameId, false);
        var items = request.Trainers.SingleOrDefault()?.Items ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, false, [.. removedItemsLogs]);
        var updatedTrainer = await TrainerService.GetTrainerById(request.UserId, request.GameId);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("remove/items/force")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerGMAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        var trainer = request.Trainers.SingleOrDefault() ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var items = trainer.Items;
        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, true, [.. removedItemsLogs]);
        var updatedTrainer = await TrainerService.GetTrainerById(trainer.TrainerId, request.GameId);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("remove/items/all")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromAllTrainersAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        var trainers = await TrainerService.GetTrainersByGameId(request.GameId);
        var items = request.Trainers.SingleOrDefault()?.Items ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        foreach (var trainer in trainers)
        {
            var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
            await GameService.UpdateGameLogs(game, true, [.. removedItemsLogs]);
        }

        var updateTrainers = await TrainerService.GetTrainersByGameId(request.GameId);
        return Ok(new UpdateTrainerResponse { Trainers = [.. updateTrainers] });
    }

    [HttpPut("add/money")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerMoney(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainer = request.Trainers.SingleOrDefault() ?? throw new InvalidTrainerException(PtaExceptionParts.TooManyShopsTrainers);
        var currentTrainer = await TrainerService.GetTrainerById(trainer.TrainerId, request.GameId);
        currentTrainer.Money += trainer.Money;
        var updatedTrainer = await TrainerService.UpdateTrainer(currentTrainer);
        return Ok(new UpdateTrainerResponse { Trainers = [updatedTrainer] });
    }

    [HttpPut("add/money/all")]
    [ProducesResponseType(typeof(UpdateTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateAllTrainersMoney(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateTrainerRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(request.GameId);
        var updatedTrainers = await Task.WhenAll(trainers.Where(trainer => !trainer.IsGM).Select(async trainer => await TrainerService.UpdateTrainer(trainer)));
        return Ok(new UpdateTrainerResponse { Trainers = [.. updatedTrainers] });
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteTrainer(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteTrainerRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        foreach (var pokemon in await PokemonService.GetPokemonByTrainerId(request.TrainerId, request.GameId))
        {
            await PokemonService.DeletePokemon(pokemon.PokemonId);
        }
        await TrainerService.DeleteTrainer(request.TrainerId, request.GameId);

        var gm = await TrainerService.GetTrainerById(request.GameMasterId, request.GameId);
        var deleteTrainerLog = new Log
        {
            User = gm.TrainerName,
            Action = GameLogMessages.DeletedPlayerLog,
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, true, deleteTrainerLog);
        return Ok();
    }

    private async Task<Trainer> CompleteTrainer(
        Guid trainerId,
        Guid gameId,
        Trainer trainerDto)
    {
        var user = await UserService.GetUserById(trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (!CheckRequestingTrainer(trainerDto, trainer, user))
        {
            throw new InvalidTrainerException(PtaExceptionParts.UnauthorizedTrainerCompletionMessage);
        }
        await AddTrainerPokemon(trainerDto.NewPokemon, trainer);
        await SetStartingEquipmentWithOrigin(trainer);
        var updatedTrainer = await TrainerService.UpdateTrainer(trainer);
        var game = await GameService.GetGame(trainer.GameId, false);
        var statsAddedLog = new Log
        {
            User = trainer.TrainerName,
            Action = GameLogMessages.UpdatedStatsLog,
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, false, statsAddedLog);
        return updatedTrainer;
    }

    private async Task AddTrainerPokemon(
        IEnumerable<NewPokemon> pokemon,
        Trainer trainer)
    {
        foreach (var data in pokemon.Where(data => data != null))
        {
            var nickname = data.Nickname.Length > 18 ? data.Nickname[..18] : data.Nickname;
            var pokemonModel = await DexService.GetNewPokemon(data.SpeciesName, nickname, data.Form);
            pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
            pokemonModel.OriginalTrainerId = trainer.TrainerId;
            pokemonModel.TrainerId = trainer.TrainerId;
            pokemonModel.GameId = trainer.GameId;
            pokemonModel.Pokeball = nameof(Pokeball.Basic_Ball).Replace("_", "");
            await PokemonService.PostPokemon(pokemonModel);
            var game = await GameService.GetGame(trainer.GameId, false);
            var caughtPokemonLog = new Log
            {
                User = trainer.TrainerName,
                Action = $"caught a {pokemonModel.SpeciesName} named {pokemonModel.Nickname}",
                LogTimestamp = DateTimeOffset.Now
            };
            await GameService.UpdateGameLogs(game, false, caughtPokemonLog);
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

    private async Task SetStartingEquipmentWithOrigin(Trainer trainer)
    {
        var origin = await DexService.GetDexEntry<OriginDto>(DexType.Origins, trainer.Origin);
        var collection = await Task.WhenAll(origin.Data.StartingEquipmentList.Select(ConvertStartingEquipment));
        trainer.Items = collection;
    }

    private async Task<ICollection<Trainer>> GetTrainers(Guid gameId)
    {
        var models = await TrainerService.GetTrainersByGameId(gameId);
        return [.. models];
    }

    [ExcludeFromCodeCoverage]
    private async Task<Item> ConvertStartingEquipment(StartingEquipment s)
    {
        var baseItem = s.Type switch
        {
            StartingEquipmentType.Trainer => await DexService.GetDexEntry<BaseItemDto>(DexType.TrainerEquipment, s.Name),
            StartingEquipmentType.Pokeball => await DexService.GetDexEntry<BaseItemDto>(DexType.Pokeballs, s.Name),
            StartingEquipmentType.Medical => await DexService.GetDexEntry<BaseItemDto>(DexType.MedicalItems, s.Name),
            StartingEquipmentType.Berry => await DexService.GetDexEntry<BaseItemDto>(DexType.Berries, s.Name),
            StartingEquipmentType.Pokemon => await DexService.GetDexEntry<BaseItemDto>(DexType.PokemonItems, s.Name),
            _ => throw new UnknownEntityException<SettingParticipantType>(nameof(s.Type), s.Type),
        };
        var item = new Item
        {
            Name = baseItem.Data.Name,
            Effects = baseItem.Data.Effects,
            Amount = s.Amount,
            Type = s.Type
        };
        return item;
    }

    #region Helper methods
    private static bool CheckRequestingTrainer(
        Trainer trainer,
        Trainer requestingTrainer,
        User user)
    {
        if (user.SiteRole == UserRoleOnSite.SiteAdmin)
        {
            return true;
        }

        if (trainer.GameId != requestingTrainer.GameId)
        {
            return false;
        }

        return trainer.TrainerId == requestingTrainer.TrainerId || requestingTrainer.IsGM;
    }
    #endregion
}
