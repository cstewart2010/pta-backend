using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/trainer")]
public class TrainerController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    ILogger<TrainerController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<TrainerController> _logger = logger;

    [HttpGet("{gameId}/trainers")]
    [ProducesResponseType(typeof(IEnumerable<Trainer>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> FindTrainers(Guid gameId)
    {
        var trainer = await GetTrainers(gameId);
        return Ok(trainer);
    }

    [HttpGet("trainers/{trainerName}")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> FindTrainer(string trainerName, [FromQuery] Guid gameId)
    {
        var trainer = await TrainerService.GetTrainerByUsername(trainerName, gameId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpGet("{gameId}/{trainerId}/{pokemonId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> FindTrainerMon(
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (pokemon.TrainerId != trainerId)
        {
            throw new PtaException($"This pokemon is not associate with trainer {trainerId}", "Invalid pokemon retrieval", HttpStatusCode.BadRequest);
        }
        if (pokemon.GameId != gameId)
        {
            throw new PtaException($"This pokemon is not associate with game {gameId}", "Invalid pokemon retrieval", HttpStatusCode.BadRequest);
        }

        return Ok(pokemon);
    }

    [HttpPost("{gameId}/{gameMasterId}/{trainerId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddPokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] WildPokemon wild)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var pokemon = await BuildPokemon(trainerId, gameId, wild);
        await PokemonService.PostPokemon(pokemon);
        await RefreshToken(gameMasterId);
        return Ok(pokemon);
    }

    [HttpPut("{gameId}/{gameMasterId}/groupHonor")]
    [ProducesResponseType(typeof(GenericResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddGroupHonor(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] string honor,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        if (string.IsNullOrEmpty(honor))
        {
            return BadRequest(nameof(honor));
        }
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        foreach (var trainer in trainers)
        {
            await TrainerService.UpdateTrainerHonors(trainer.TrainerId, trainer.GameId, trainer.Honors.Append(honor));
        }

        var updatedHonorsLog = new LogModel
        (
            user: "The party",
            action: $"has earned a new honor: {honor}"
        );

        await GameService.UpdateGameLogs(game, updatedHonorsLog);
        await RefreshToken(gameMasterId);
        return Ok(new GenericResponse($"Granted the party honor: {honor}"));
    }

    [HttpPut("{gameId}/{gameMasterId}/honor")]
    [ProducesResponseType(typeof(GenericResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddSingleHonor(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        PutSungleHonorRequest request,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await TrainerService.UpdateTrainerHonors(request.TrainerId, trainer.GameId, trainer.Honors.Append(request.Honor));
        var updatedHonorsLog = new LogModel(
            user: "The Game Master",
            action: $"has granted {trainer.TrainerName} a new honor");
        await GameService.UpdateGameLogs(game, updatedHonorsLog);
        await RefreshToken(gameMasterId);
        return Ok(new GenericResponse($"Granted {gameMasterId} honor: {request.Honor}"));
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/addItems")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToTrainerAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        var addedItemsLogs = await AddItemsToTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. addedItemsLogs]);
        await RefreshToken(gameMasterId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/addItems/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToAllTrainersAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var game = await GameService.GetGame(gameId);
        if (trainers.Any())
        {
            foreach (var trainer in trainers)
            {
                var addedItemsLogs = await AddItemsToTrainer(trainer, items);
                await GameService.UpdateGameLogs(game, [.. addedItemsLogs]);
            }
        }

        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/removeItems")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        if (trainer?.IsOnline != true)
        {
            throw new PtaException($"User {trainerId} isn't online", "Invalid item purchase request", HttpStatusCode.BadRequest);
        }

        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
        await RefreshToken(trainerId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/removeItems")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerGMAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (trainer?.IsOnline != true)
        {
            throw new PtaException($"User {trainerId} isn't online", "Invalid item purchase request", HttpStatusCode.BadRequest);
        }

        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
        await RefreshToken(gameMasterId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/removeItems/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromAllTrainersAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId, 
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        if (trainers.Any())
        {
            foreach (var trainer in trainers)
            {
                var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
                await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
            }
        }

        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/money")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerMoney(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] int addition)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        trainer.Money += addition;
        await TrainerService.UpdateTrainer(trainer);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/money/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateAllTrainersMoney(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] int addition)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        foreach (var trainer in trainers.Where(trainer => !trainer.IsGM))
        {
            trainer.Money += addition;
            await TrainerService.UpdateTrainer(trainer);
        }

        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}/{trainerId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        foreach (var pokemon in await PokemonService.GetPokemonByTrainerId(trainerId))
        {
            await PokemonService.DeletePokemon(pokemon.PokemonId);
        }
        await TrainerService.DeleteTrainer(gameId, trainerId);
        if (await TrainerService.GetTrainerById(trainerId, gameId) == null)
        {
            throw new PtaException($"Failed to delete trainer {trainerId}", "Deletion fail", HttpStatusCode.BadRequest);
        }

        var deleteTrainerLog = new LogModel(
            user: "The Game Master",
            action: $"removed a trainer and all of their pokemon from the game");
        await GameService.UpdateGameLogs(game, deleteTrainerLog);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    private async Task<List<Trainer>> GetTrainers(Guid gameId)
    {
        var models = await TrainerService.GetTrainersByGameId(gameId);
        var trainers = await Task.WhenAll(models.Select(async trainer => await Trainer.ParseFromModel(trainer, PokemonService, PokedexService)));
        return [.. trainers];
    }
}
