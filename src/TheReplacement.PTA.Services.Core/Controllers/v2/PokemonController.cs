using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/pokemon")]
public class PokemonController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IPokedexService pokedexService,
    IGameService gameService,
    IDexService dexUtility,
    IEncryptionService encryptionService,
    ILogger<PokemonController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<PokemonController> _logger = logger;

    [HttpGet("{pokemonId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemon(Guid pokemonId)
    {
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        return Ok(pokemon);
    }

    [HttpGet("{gameId}/{trainerId}/{pokemonId}/possibleEvolutions")]
    [ProducesResponseType(typeof(IEnumerable<BasePokemonModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetPossibleEvolutions(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException("This pokemon can only be accessed by it's trainer or the game master");
        }
        var models = await DexService.GetPossibleEvolutions(pokemon);
		return Ok(models.ToList());
    }

    [HttpPatch("{gameId}/{gameMasterId}/trade")]
    [ProducesResponseType(typeof(TradePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> TradePokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] Guid leftPokemonId,
        [FromQuery] Guid rightPokemonId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var (leftPokemon, rightPokemon) = await GetTradePokemon(leftPokemonId, rightPokemonId);
        await UpdatePokemonTrainerIds(leftPokemon, rightPokemon);
        var leftTrainer = await TrainerService.GetTrainerById(rightPokemon.TrainerId, gameId);
        var rightTrainer = await TrainerService.GetTrainerById(leftPokemon.TrainerId, gameId);
        var tradeLog = new LogModel(
            user: "The Game Master",
            action: $"authorized a trade between {leftTrainer.TrainerName} and {rightTrainer.TrainerName}");
        await GameService.UpdateGameLogs(game, tradeLog);
        await RefreshToken(gameMasterId);
        return Ok(new TradePokemonResponse
        {
            LeftPokemon = leftPokemon,
            RightPokemon = rightPokemon
        });
    }

    [HttpPatch("{gameId}/{trainerId}/{pokemonId}/hp/{hp}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> UpdateHP(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId,
        int hp)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (!(pokemon.TrainerId == trainerId || trainer.IsGM == true))
        {
            throw new PtaUnauthorizedException("Only the pokemon's trainer or the game master can update pokemon's health");
        }

        OutofRangeException.CheckValue(-pokemon.PokemonStats.HP, pokemon.PokemonStats.HP, hp);
        await PokemonService.UpdatePokemonHP(pokemonId, hp);
        return Ok();
    }

    [HttpPatch("{gameId}/{trainerId}/{pokemonId}/form/{form}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SwitchForm(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId,
        string form)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var game = await GameService.GetGame(gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException("This pokemon can only be accessed by it's trainer or the game master");
        }

        form = form.Replace('_', '/');
        if (!pokemon.AlternateForms.Contains(form))
        {
            throw new PtaException($"Invalid form for {pokemon.SpeciesName}", "Invalid Form", HttpStatusCode.BadRequest);
        }

        var result = await GetDifferentForm(pokemon, form);
        result.PokemonId = pokemon.PokemonId;
        result.OriginalTrainerId = pokemon.OriginalTrainerId;
        result.TrainerId = pokemon.TrainerId;
        result.IsOnActiveTeam = pokemon.IsOnActiveTeam;
        result.IsShiny = pokemon.IsShiny;
        result.CanEvolve = pokemon.CanEvolve;
        result.Pokeball = pokemon.Pokeball;
        await PokemonService.UpdatePokemon(result);
        var changedFormLog = new LogModel(
            user: trainer.TrainerName,
            action: $"changed their {pokemon.Nickname} to its {form} form");
        await GameService.UpdateGameLogs(game, changedFormLog);
        await RefreshToken(trainerId);
        return Ok(result);
    }

    [HttpPatch("{gameId}/{gameMasterId}/{pokemonId}/canEvolve")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> MarkPokemonAsEvolvable(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId, 
        Guid pokemonId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        var trainer = await TrainerService.GetTrainerById(pokemon.TrainerId, gameId);
        await PokemonService.UpdatePokemonEvolvability(pokemonId, true);
        var evolutionLog = new LogModel(
            user: trainer.TrainerName,
            action: $"can now evolve their {pokemon.Nickname}");
        await GameService.UpdateGameLogs(game, evolutionLog);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/{pokemonId}/evolve")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EvolvePokemonAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] EvolvePokemonRequest request,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {

        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var game = await GameService.GetGame(gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException("This pokemon can only be accessed by it's trainer or the game master");
        }
        var evolvedForm = await GetEvolved(pokemon, request);

        await PokemonService.UpdatePokemon(evolvedForm);
        var evolutionLog = new LogModel(
            user: trainer.TrainerName,
            action: $"evolved their {pokemon.Nickname} to an {evolvedForm.SpeciesName}");
        await GameService.UpdateGameLogs(game, evolutionLog);
        var dexItem = await PokedexService.GetPokedexItem(trainerId, gameId, evolvedForm.DexNo);
        if (dexItem != null)
        {
            if (!dexItem.IsCaught)
            {
                await PokedexService.UpdateDexItemIsCaught(trainerId, gameId, evolvedForm.DexNo);
            }
        }
        else
        {
            await PokedexService.UpdateDexItemIsCaught(trainerId, gameId, evolvedForm.DexNo);
        }
        await RefreshToken(trainerId);
        return Ok(pokemon);
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/saw")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsSeen(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] int dexNo)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var dexItem = await PokedexService.GetPokedexItem(trainerId, gameId, dexNo);
        if (dexItem != null)
        {
            if (dexItem.IsSeen)
            {
                throw new PtaException("This pokemon has already been registered as seend", "Invalid pokedex registration", HttpStatusCode.BadRequest);
            }
            await PokedexService.UpdateDexItemIsSeen(trainerId, gameId, dexNo);
            return Ok();
        }

        await AddDexItem(trainerId, gameId, dexNo, isSeen: true);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/caught")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsCaught(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] int dexNo)
    {
       await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var dexItem = await PokedexService.GetPokedexItem(trainerId, gameId, dexNo);
        if (dexItem != null)
        {
            if (dexItem.IsCaught)
            {
                throw new PtaException("This pokemon has already been registered as caught", "Invalid pokedex registration", HttpStatusCode.BadRequest);
            }
            await PokedexService.UpdateDexItemIsCaught(trainerId, gameId, dexNo);
            return Ok();
        }

        await AddDexItem(trainerId, gameId, dexNo, isCaught: true);
        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}/{pokemonId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> DeletePokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid pokemonId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await PokemonService.DeletePokemon(pokemonId);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    private async Task<PokemonModel> GetDifferentForm(PokemonModel pokemon, string form)
    {
        return await DexService.GetNewPokemon(
            pokemon.SpeciesName,
            Enum.Parse<Nature>(pokemon.Nature),
            Enum.Parse<Gender>(pokemon.Gender),
            Enum.Parse<Status>(pokemon.PokemonStatus),
            pokemon.Nickname,
            form);
    }

    private async Task<ActionResult<AbstractDto>> AddDexItem(Guid trainerId, Guid gameId, int dexNo, bool isSeen = false, bool isCaught = false)
    {
        if (isCaught)
        {
            isSeen = true;
        }

        await PokedexService.PostDexItem(
            trainerId,
            gameId,
            dexNo,
            isSeen,
            isCaught);
        return new GenericResponse("Pokedex item added successfully");
    }

    private async Task UpdatePokemonTrainerIds(
        PokemonModel leftPokemon,
        PokemonModel rightPokemon)
    {

        await PokemonService.UpdatePokemonTrainerId
        (
            leftPokemon.PokemonId,
            rightPokemon.TrainerId
        );

        await PokemonService.UpdatePokemonLocation
        (
            leftPokemon.PokemonId,
            rightPokemon.IsOnActiveTeam
        );

        await PokemonService.UpdatePokemonTrainerId
        (
            rightPokemon.PokemonId,
            leftPokemon.TrainerId
        );

        await PokemonService.UpdatePokemonLocation
        (
            rightPokemon.PokemonId,
            leftPokemon.IsOnActiveTeam
        );
    }

    private async Task<(PokemonModel LeftPokemon, PokemonModel RightPokemon)> GetTradePokemon(
        Guid leftPokemonId,
        Guid rightPokemonId)
    {
        var leftPokemon = await PokemonService.GetPokemonById(leftPokemonId);
        var rightPokemon = await PokemonService.GetPokemonById(rightPokemonId);
        if (leftPokemon.TrainerId == rightPokemon.TrainerId)
        {
            throw new PtaException("Cannot trade with oneself", "Invalid trade request", HttpStatusCode.BadRequest);
        }

        return (leftPokemon, rightPokemon);
    }

    private async Task<PokemonModel> GetEvolved(
        PokemonModel currentForm,
        EvolvePokemonRequest request)
    {
        var total = request.KeptMoves.Count() + request.NewMoves.Count();
        OutofRangeException.CheckValue(3, 6, total);

        var moveComparer = currentForm.Moves.Select(move => move.ToLower());
        if (!request.KeptMoves.All(move => moveComparer.Contains(move.ToLower())))
        {
            throw new PtaException($"{currentForm.Nickname} doesn't contain one of {string.Join(", ", request.KeptMoves)}", "Invalid Request", HttpStatusCode.BadRequest);
        }

        var evolvedForm = await DexService.GetEvolved(currentForm, request.KeptMoves, request.NextForm, request.NewMoves);
        evolvedForm.Pokeball = currentForm.Pokeball;
        return evolvedForm;
    }
}
