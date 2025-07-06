using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.NpcRoute)]
public class NpcController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    INpcService npcService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    ILogger<NpcController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly INpcService _npcService = npcService;
    private readonly ILogger<NpcController> _logger = logger;

    [HttpGet("{gameId}/{gameMasterId}/{npcId}")]
    [ProducesResponseType(typeof(Npc), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpc(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid npcId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var npc = await _npcService.GetNpc(npcId);
        var gameMaster = await TrainerService.GetTrainerById(gameMasterId, gameId);
        if (gameMaster.GameId != npc.GameId)
        {
            return Conflict();
        }

        return Ok(await Npc.ParseFromModel(npc, PokemonService));
    }


    [HttpGet("{gameId}/{gameMasterId}/{npcId}/{pokemonId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpcMon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameMasterId,
        Guid gameId,
        Guid npcId,
        Guid pokemonId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var pokemon = (await PokemonService.GetPokemonByTrainerId(npcId)).SingleOrDefault(pokemon => pokemon.PokemonId == pokemonId);
        if (pokemon == null)
        {
            return NotFound(pokemonId);
        }

        return Ok(pokemon);
    }


    [HttpGet("{gameId}/{gameMasterId}/npcs/all")]
    [ProducesResponseType(typeof(IEnumerable<Npc>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult<IEnumerable<Npc>>> GetNpcsInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameMasterId,
        Guid gameId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var models = await _npcService.GetNpcsByGameId(gameId);
        var npcs = await Task.WhenAll(models.Select(async npc => await Npc.ParseFromModel(npc, PokemonService)));
        return Ok(npcs);
    }

    [HttpPost("{gameId}/{gameMasterId}/new")]
    [ProducesResponseType(typeof(Npc), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewNpcAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] PostNpcRequest request,
        Guid gameMasterId,
        Guid gameId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var npc = await CreateNpc(request);
        npc.GameId = gameId;
        await _npcService.PostNpc(npc);
        return Ok(Npc.ParseFromModel(npc, PokemonService));
    }

    [HttpPost("{gameId}/{gameMasterId}/{npcId}/new")]
    [ProducesResponseType(typeof(IEnumerable<PokemonModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewNpcMonAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<NewPokemon> newPokemon,
        Guid gameMasterId,
        Guid gameId,
        Guid npcId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var pokemon = await AddNpcPokemon(newPokemon, npcId, gameId);
        return Ok(pokemon);
    }


    [HttpPatch("{gameId}/{gameMasterId}/addStats")]
    [ProducesResponseType(typeof(Npc), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddNpcStats(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] Npc npcRequest,
        Guid gameMasterId,
        Guid gameId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var npc = await npcRequest.ParseBackToModel(_npcService);
        await _npcService.UpdateNpc(npc);
        return Ok(Npc.ParseFromModel(npc, PokemonService));
    }

    [HttpDelete("{gameId}/{gameMasterId}/{npcId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteNpc(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameMasterId,
        Guid gameId,
        Guid npcId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var npc = await _npcService.GetNpc(npcId);
        if (gameId != npc.GameId)
        {
            return Conflict();
        }

        await _npcService.DeleteNpc(npcId);
        return Ok();
    }

    private async Task<NpcModel> CreateNpc(PostNpcRequest request)
    {
        var trainerName = request.TrainerName;
        var feats = (await Task.WhenAll(request.Feat.Select(async feat => await DexService.GetDexEntry<FeatureModel>(DexType.Features, feat.ToString()))
            .Where(feat => feat != null)))
            .Select(feat => feat.Data.Name);

        var classes = (await Task.WhenAll(request.Classes.Select(@class => DexService.GetDexEntry<TrainerClassModel>(DexType.TrainerClasses, @class.ToString()))
            .Where(@class => @class != null)))
            .Select(@class => @class.Data.Name);

        // add gameMaster's GameId to npc
        return new NpcModel
        {
            NPCId = Guid.NewGuid(),
            Feats = feats,
            TrainerClasses = classes,
            TrainerName = trainerName,
            TrainerStats = new StatsModel(),
            CurrentHP = 0,
            Sprite = "acetrainer"
        };
    }

    [HttpDelete("{gameId}/{gameMasterId}/npcs/all")]
    public async Task<ActionResult> DeleteNpcsInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameMasterId,
        Guid gameId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await _npcService.DeleteNpcByGameId(gameId);
        return Ok();
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
}
