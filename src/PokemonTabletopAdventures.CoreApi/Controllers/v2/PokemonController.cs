using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Pokedex;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Trainers;
using System.Net;

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
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<PokemonController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    [HttpPost("retrieve")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemon(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.TrainerId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        return Ok(new RetrievePokemonResponse { Pokemon = [model] });
    }

    [HttpGet("retrieve/trainer")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> GetTrainerMon(
        [FromHeader] RetrievePokemonRequest request)
    {
        var models = await PokemonService.GetPokemonByTrainerId(request.TrainerId, request.GameId);
        return Ok(new RetrievePokemonResponse { Pokemon = [.. models] });
    }

    [HttpPost("retrieve/npc")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpcMon(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        var models = await PokemonService.GetPokemonByTrainerId(request.TrainerId, request.GameId);
        return Ok(new RetrievePokemonResponse { Pokemon = [..models] });
    }

    [HttpPost("evolutions")]
    [ProducesResponseType(typeof(RetrievePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetPossibleEvolutions(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromHeader] RetrievePokemonRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.TrainerId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var pokemon = await PokemonService.GetPokemonById(request.PokemonId);
        CheckTrainerIds(trainer, pokemon);
        var models = await DexService.GetPossibleEvolutions(pokemon);
		return Ok(new RetrievePokemonResponse { Models = [.. models] });
    }

    [HttpPatch("trade")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> TradePokemon(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] TradePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        var (leftPokemon, rightPokemon) = await GetTradePokemon(request.LeftPokemonId, request.RightPokemonId);
        await UpdatePokemonTrainerIds(leftPokemon, rightPokemon);
        var leftTrainer = await TrainerService.GetTrainerById(rightPokemon.TrainerId, request.GameId);
        var rightTrainer = await TrainerService.GetTrainerById(leftPokemon.TrainerId, request.GameId);
        var gm = await TrainerService.GetTrainerById(request.GameMasterId, request.GameId);
        var tradeLog = new Log
        {
            User = gm.TrainerName,
            Action = $"authorized a trade between {leftTrainer.TrainerName} and {rightTrainer.TrainerName}",
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, true, tradeLog);
        return Ok();
    }

    [HttpPost("capture")]
    [ProducesResponseType(typeof(CapturePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> CapturePokemon(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CapturePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        var model = await BuildPokemon(request.TrainerId, request.GameId, request.Pokemon);
        await PokemonService.PostPokemon(model);
        return Ok(new CapturePokemonResponse { Pokemon = model });
    }

    [HttpPost("create/npc")]
    [ProducesResponseType(typeof(CreatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> CreateNewNpcMonAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreatePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        var models = await AddNpcPokemon(request.Pokemon, request.TrainerId, request.GameId);
        return Ok(new CreatePokemonResponse { Pokemon = [.. models] });
    }

    [HttpPatch("update/hp")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult> UpdateHP(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.TrainerId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        CheckTrainerIds(trainer, model);
        OutofRangeException.CheckValue(-model.PokemonStats.HP, model.PokemonStats.HP, request.HP);
        var updatedModel = await PokemonService.UpdatePokemonHP(request.PokemonId, request.HP);
        return Ok(new UpdatePokemonResponse { Pokemon = updatedModel });
    }

    [HttpPatch("update/form")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> SwitchForm(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        var form = request.Form!;
        var isGm = await VerifyIdentity(sessionAuth, logger, request.TrainerId, request.GameId);
        var game = await GameService.GetGame(request.GameId, isGm);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        CheckTrainerIds(trainer, model);
        form = form.Replace('_', '/');
        if (!model.AlternateForms.Contains(form))
        {
            throw new PtaException($"Invalid form for {model.SpeciesName}", PtaExceptionParts.InvalidFormTitle, HttpStatusCode.BadRequest);
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
        var changedFormLog = new Log
        {
            User = trainer.TrainerName,
            Action = $"changed their {model.Nickname} to its {form} form",
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, isGm, changedFormLog);
        return Ok(new UpdatePokemonResponse { Pokemon = updatedModel });
    }

    [HttpPatch("canEvolve")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> MarkPokemonAsEvolvable(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        var trainer = await TrainerService.GetTrainerById(model.TrainerId, request.GameId);
        var updatedModel = await PokemonService.UpdatePokemonEvolvability(request.PokemonId, true);
        var evolutionLog = new Log
        {
            User = trainer.TrainerName,
            Action = $"can now evolve their {model.Nickname}",
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, true, evolutionLog);
        return Ok(new UpdatePokemonResponse { Pokemon = updatedModel });
    }

    [HttpPut("evolve")]
    [ProducesResponseType(typeof(UpdatePokemonResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> EvolvePokemonAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokemonRequest request)
    {
        if (request.EvolvePokemonData == null)
        {
            throw new InvalidEvolutionException(PtaExceptionParts.NullDataMessage);
        }
        var isGm = await VerifyIdentity(sessionAuth, logger, request.TrainerId, request.GameId);
        var game = await GameService.GetGame(request.GameId, isGm);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var model = await PokemonService.GetPokemonById(request.PokemonId);
        CheckTrainerIds(trainer, model);
        var evolvedForm = await GetEvolved(model, request.EvolvePokemonData);

        var updatedModel = await PokemonService.UpdatePokemon(evolvedForm);
        var evolutionLog = new Log
        {
            User = trainer.TrainerName,
            Action = "",
            LogTimestamp = DateTimeOffset.UtcNow
        };
        await GameService.UpdateGameLogs(game, isGm, evolutionLog);
        await PokedexService.UpdateDexItemIsCaught(request.TrainerId, request.GameId, evolvedForm.DexNo);
        return Ok(new UpdatePokemonResponse { Pokemon = updatedModel });
    }

    [HttpPatch("saw")]
    [ProducesResponseType(typeof(UpdatePokedexResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsSeen(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokedexRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        foreach (var item in request.PokedexItems)
        {
            var dexItem = await PokedexService.GetPokedexItem(request.TrainerId, request.GameId, item.DexNo);
            if (dexItem != null)
            {
                await PokedexService.UpdateDexItemIsSeen(request.TrainerId, request.GameId, item.DexNo);
            }
            else
            {
                await AddDexItem(request.TrainerId, request.GameId, item.DexNo, isSeen: true);   
            }
        }
        var pokedexModel = await PokedexService.GetTrainerPokeDex(request.TrainerId, request.GameId);
        return Ok(new UpdatePokedexResponse { PokedexItems = [.. pokedexModel] });
    }

    [HttpPatch("caught")]
    [ProducesResponseType(typeof(UpdatePokedexResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateDexItemIsCaught(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdatePokedexRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        foreach (var item in request.PokedexItems)
        {
            var dexItem = await PokedexService.GetPokedexItem(request.TrainerId, request.GameId, item.DexNo);
            if (dexItem != null)
            {
                await PokedexService.UpdateDexItemIsCaught(request.TrainerId, request.GameId, item.DexNo);
            }
            else
            {
                await AddDexItem(request.TrainerId, request.GameId, item.DexNo, isCaught: true);   
            }
        }
        var pokedexModel = await PokedexService.GetTrainerPokeDex(request.TrainerId, request.GameId);
        return Ok(new UpdatePokedexResponse { PokedexItems = [.. pokedexModel] });
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> DeletePokemon(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeletePokemonRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        await PokemonService.DeletePokemon(request.PokemonId);
        return Ok();
    }

    private async Task<Pokemon> GetDifferentForm(Pokemon pokemon, string form)
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

    private async Task<IEnumerable<Pokemon>> AddNpcPokemon(IEnumerable<NewPokemon> pokemon, Guid npcId, Guid gameId)
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
        Pokemon leftPokemon,
        Pokemon rightPokemon)
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

    private async Task<(Pokemon LeftPokemon, Pokemon RightPokemon)> GetTradePokemon(
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

    private async Task<Pokemon> GetEvolved(
        Pokemon currentForm,
        EvolvePokemonData request)
    {
        var total = request.KeptMoves.Count + request.NewMoves.Count;
        OutofRangeException.CheckValue(1, 6, total);

        var moveComparer = currentForm.Moves.Select(move => move.ToLower());
        if (!request.KeptMoves.All(move => moveComparer.Contains(move.ToLower())))
        {
            throw new InvalidEvolutionException($"{currentForm.Nickname} doesn't contain one of {string.Join(", ", request.KeptMoves)}");
        }

        var evolvedForm = await DexService.GetEvolved(currentForm, [..request.KeptMoves], request.NextForm, [..request.NewMoves]);
        evolvedForm.Pokeball = currentForm.Pokeball;
        return evolvedForm;
    }

    private static void CheckTrainerIds(Trainer trainer, Pokemon pokemon)
    {
        if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.UnauthorizedPokemonUseMessage);
        }
    }
}
