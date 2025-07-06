using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Games;
using PokemonTabletopAdventures.CoreApi.DTOs.Pokedex;
using PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.PokemonRoute)]
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

    [HttpPost("retrieve")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        var pokemon = ParseFromModel(model);
        return Ok(new RetrievePokemonResponse { Pokemon = [pokemon] });
    }


    [HttpPost("retrieve/npc")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpcMon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var models = await PokemonService.GetPokemonByTrainerId(request.TrainerId);
        var pokemon = models.Select(ParseFromModel).ToList();
        return Ok(new RetrievePokemonResponse { Pokemon = pokemon });
    }

    [HttpPost("evolutions")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetPossibleEvolutions(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var pokemon = await PokemonService.GetPokemonById(request.PokemonId);
        if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.UnauthorizedPokemonUseMessage);
        }
        var models = await DexService.GetPossibleEvolutions(pokemon);
		return Ok(new RetrievePokemonResponse { Models = models.ToList() });
    }

    [HttpPatch("trade")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> TradePokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] TradePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(request.GameId);
        var (leftPokemon, rightPokemon) = await GetTradePokemon(request.LeftPokemonId, request.RightPokemonId);
        await UpdatePokemonTrainerIds(leftPokemon, rightPokemon);
        var leftTrainer = await TrainerService.GetTrainerById(rightPokemon.TrainerId, request.GameId);
        var rightTrainer = await TrainerService.GetTrainerById(leftPokemon.TrainerId, request.GameId);
        var gm = await TrainerService.GetTrainerById(request.GameMasterId, request.GameId);
        var tradeLog = new LogModel(
            user: gm.TrainerName,
            action: $"authorized a trade between {leftTrainer.TrainerName} and {rightTrainer.TrainerName}");
        await GameService.UpdateGameLogs(game, tradeLog);
        await RefreshToken(request.GameMasterId);
        return Ok();
    }

    [HttpPost("capture")]
    [ProducesResponseType(typeof(CapturePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CapturePokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CapturePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var model = await BuildPokemon(request.TrainerId, request.GameId, request.Pokemon);
        await PokemonService.PostPokemon(model);
        await RefreshToken(request.GameMasterId);
        var pokemon = ParseFromModel(model);
        return Ok(new CapturePokemonResponse { Pokemon = pokemon });
    }

    [HttpPost("create/npc")]
    [ProducesResponseType(typeof(CreatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewNpcMonAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreatePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var models = await AddNpcPokemon(request.Pokemon, request.TrainerId, request.GameId);
        var pokemon = models.Select(ParseFromModel).ToList();
        return Ok(new CreatePokemonResponse { Pokemon = pokemon });
    }

    [HttpPatch("update/hp")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> UpdateHP(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        if (!(model.TrainerId == request.TrainerId || trainer.IsGM == true))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.UnauthorizedPokemonUseMessage);
        }

        OutofRangeException.CheckValue(-model.PokemonStats.HP, model.PokemonStats.HP, request.HP);
        var updatedModel = await PokemonService.UpdatePokemonHP(request.PokemonId, request.HP);
        var pokemon = ParseFromModel(updatedModel);
        return Ok(new UpdatePokemonResponse { Pokemon = pokemon });
    }

    [HttpPatch("update/form")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SwitchForm(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        var form = request.Form!;
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var game = await GameService.GetGame(request.GameId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        if (!(trainer.TrainerId == model.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.UnauthorizedPokemonUseMessage);
        }

        form = form.Replace('_', '/');
        if (!model.AlternateForms.Contains(form))
        {
            throw new PtaException($"Invalid form for {model.SpeciesName}", "Invalid Form", HttpStatusCode.BadRequest);
        }

        var result = await GetDifferentForm(model, form);
        result.PokemonId = model.PokemonId;
        result.OriginalTrainerId = model.OriginalTrainerId;
        result.TrainerId = model.TrainerId;
        result.IsOnActiveTeam = model.IsOnActiveTeam;
        result.IsShiny = model.IsShiny;
        result.CanEvolve = model.CanEvolve;
        result.Pokeball = model.Pokeball;
        var updatedModel = await PokemonService.UpdatePokemon(result);
        var pokemon = ParseFromModel(updatedModel);
        var changedFormLog = new LogModel(
            user: trainer.TrainerName,
            action: $"changed their {model.Nickname} to its {form} form");
        await GameService.UpdateGameLogs(game, changedFormLog);
        await RefreshToken(request.TrainerId);
        return Ok(new UpdatePokemonResponse { Pokemon = pokemon });
    }

    [HttpPatch("canEvolve")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> MarkPokemonAsEvolvable(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(request.GameId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        var trainer = await TrainerService.GetTrainerById(model.TrainerId, request.GameId);
        var updatedModel = await PokemonService.UpdatePokemonEvolvability(request.PokemonId, true);
        var pokemon = ParseFromModel(updatedModel);
        var evolutionLog = new LogModel(
            user: trainer.TrainerName,
            action: $"can now evolve their {model.Nickname}");
        await GameService.UpdateGameLogs(game, evolutionLog);
        await RefreshToken(request.GameMasterId);
        return Ok(new UpdatePokemonResponse { Pokemon = pokemon });
    }

    [HttpPut("evolve")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EvolvePokemonAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        if (request.EvolvePokemonData == null)
        {
            throw new InvalidEvolutionException("EvolvePokemonData property was null");
        }
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var game = await GameService.GetGame(request.GameId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        if (!(trainer.TrainerId == model.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.UnauthorizedPokemonUseMessage);
        }
        var evolvedForm = await GetEvolved(model, request.EvolvePokemonData);

        var updatedModel = await PokemonService.UpdatePokemon(evolvedForm);
        var pokemon = ParseFromModel(updatedModel);
        var evolutionLog = new LogModel(
            user: trainer.TrainerName,
            action: $"evolved their {model.Nickname} to an {evolvedForm.SpeciesName}");
        await GameService.UpdateGameLogs(game, evolutionLog);
        var dexItem = await PokedexService.GetPokedexItem(request.TrainerId, request.GameId, evolvedForm.DexNo);
        if (dexItem != null)
        {
            if (!dexItem.IsCaught)
            {
                await PokedexService.UpdateDexItemIsCaught(request.TrainerId, request.GameId, evolvedForm.DexNo);
            }
        }
        else
        {
            await PokedexService.UpdateDexItemIsCaught(request.TrainerId, request.GameId, evolvedForm.DexNo);
        }
        await RefreshToken(request.TrainerId);
        return Ok(new UpdatePokemonResponse { Pokemon = pokemon });
    }

    [HttpPatch("saw")]
    [ProducesResponseType(typeof(UpdatePokedexResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsSeen(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokedexRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        foreach (var item in request.PokedexItems)
        {
            var dexItem = await PokedexService.GetPokedexItem(request.TrainerId, request.GameId, item.DexNo);
            if (dexItem != null)
            {
                await PokedexService.UpdateDexItemIsSeen(request.TrainerId, request.GameId, item.DexNo);
                return Ok();
            }

            await AddDexItem(request.TrainerId, request.GameId, item.DexNo, isSeen: true);
        }
        var pokedexModel = await PokedexService.GetTrainerPokeDex(request.TrainerId, request.GameId);
        var pokedex = pokedexModel.Select(ParseFromModel).OrderBy(item => item.DexNo);
        return Ok(new UpdatePokedexResponse { PokedexItems = [.. pokedex] });
    }

    // todo: make a pokedex controller
    [HttpPatch("caught")]
    [ProducesResponseType(typeof(UpdatePokedexResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsCaught(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokedexRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        foreach (var item in request.PokedexItems)
        {
            var dexItem = await PokedexService.GetPokedexItem(request.TrainerId, request.GameId, item.DexNo);
            if (dexItem != null)
            {
                await PokedexService.UpdateDexItemIsCaught(request.TrainerId, request.GameId, item.DexNo);
                return Ok();
            }

            await AddDexItem(request.TrainerId, request.GameId, item.DexNo, isCaught: true);
        }
        var pokedexModel = await PokedexService.GetTrainerPokeDex(request.TrainerId, request.GameId);
        var pokedex = pokedexModel.Select(ParseFromModel).OrderBy(item => item.DexNo);
        return Ok(new UpdatePokedexResponse { PokedexItems = [.. pokedex] });
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> DeletePokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeletePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        await PokemonService.DeletePokemon(request.PokemonId);
        await RefreshToken(request.GameMasterId);
        return Ok();
    }

    private async Task<PokemonModel> GetDifferentForm(PokemonModel pokemon, string form)
    {
        return await DexService.GetNewPokemon(
            pokemon.SpeciesName,
            pokemon.Nature,
            pokemon.Gender,
            pokemon.PokemonStatus,
            pokemon.Nickname,
            form);
    }

    private async Task AddDexItem(Guid trainerId, Guid gameId, int dexNo, bool isSeen = false, bool isCaught = false)
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
    }

    private async Task<IEnumerable<PokemonModel>> AddNpcPokemon(IEnumerable<NewPokemon> pokemon, Guid npcId, Guid gameId)
    {
        var models = await Task.WhenAll(pokemon
            .Where(data => data != null)
            .Select(async data =>
            {
                var nickname = data.Nickname.Length > 18 ? data.Nickname[..18] : data.Nickname;
                var pokemonModel = await DexService.GetNewPokemon(data.SpeciesName, nickname, data.Form);
                pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
                pokemonModel.OriginalTrainerId = npcId;
                pokemonModel.TrainerId = npcId;
                pokemonModel.GameId = gameId;
                await PokemonService.PostPokemon(pokemonModel);
                return pokemonModel;
            }));

        return models;
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
            throw new InvalidTradeException();
        }

        return (leftPokemon, rightPokemon);
    }

    private async Task<PokemonModel> GetEvolved(
        PokemonModel currentForm,
        EvolvePokemonData request)
    {
        var total = request.KeptMoves.Count() + request.NewMoves.Count();
        OutofRangeException.CheckValue(3, 6, total);

        var moveComparer = currentForm.Moves.Select(move => move.ToLower());
        if (!request.KeptMoves.All(move => moveComparer.Contains(move.ToLower())))
        {
            throw new Exceptions.PtaException($"{currentForm.Nickname} doesn't contain one of {string.Join(", ", request.KeptMoves)}", "Invalid Request", HttpStatusCode.BadRequest);
        }

        var evolvedForm = await DexService.GetEvolved(currentForm, request.KeptMoves, request.NextForm, request.NewMoves);
        evolvedForm.Pokeball = currentForm.Pokeball;
        return evolvedForm;
    }
}
