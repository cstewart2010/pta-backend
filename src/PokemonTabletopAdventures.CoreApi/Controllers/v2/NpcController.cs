using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;

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
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<NpcController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    private readonly INpcService _npcService = npcService;
    private readonly ILogger<NpcController> _logger = logger;

    [HttpPost("retrieve")]
    [ProducesResponseType(typeof(RetrieveNpcResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpc(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var model = await _npcService.GetNpc(request.NpcId);
        var gameMaster = await TrainerService.GetTrainerById(request.GameMasterId, request.GameId);
        if (gameMaster.GameId != model.GameId)
        {
            return Conflict();
        }

        return Ok(new RetrieveNpcResponse { Npcs = [model] });
    }

    [HttpPost("retrieve/all")]
    [ProducesResponseType(typeof(RetrieveNpcResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetNpcsInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var models = await _npcService.GetNpcsByGameId(request.GameId);
        return Ok(new RetrieveNpcResponse { Npcs = [..models] });
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(CreateNpcResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewNpcAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreateNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        foreach (var npc in request.Npcs)
        {
            npc.NpcId = Guid.NewGuid();
            await _npcService.PostNpc(npc);
        }
        return Ok(new CreateNpcResponse { Npcs = request.Npcs });
    }


    [HttpPut("update")]
    [ProducesResponseType(typeof(Npc), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddNpcStats(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var updatedList = await Task.WhenAll(request.Npcs.Select(async model => await _npcService.UpdateNpc(model)));
        return Ok(new UpdateNpcResponse { Npcs = updatedList });
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteNpc(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var npc = await _npcService.GetNpc(request.NpcId);
        if (request.GameId != npc.GameId)
        {
            return Conflict();
        }

        await _npcService.DeleteNpc(request.NpcId);
        var game = await GameService.GetGame(npc.GameId, true);
        var npcList = game.Npcs.Select(npc => npc.NpcId).Where(npcId => request.NpcId != npcId);
        await GameService.UpdateGameNpcList(npc.GameId, [.. npcList]);
        return Ok();
    }

    [HttpDelete("delete/all")]
    public async Task<ActionResult> DeleteNpcsInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        foreach (var npc in game.Npcs)
        {
            await _npcService.DeleteNpc(npc.NpcId);
        }
        await GameService.UpdateGameNpcList(request.GameId, []);
        return Ok();
    }
}
