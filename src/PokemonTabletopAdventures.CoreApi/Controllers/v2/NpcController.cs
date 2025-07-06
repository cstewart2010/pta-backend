using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.Services;
using System;
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

        var npc = await ParseFromModel(model);
        return Ok(new RetrieveNpcResponse { Npcs = [npc] });
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
        var npcs = await Task.WhenAll(models.Select(async npc => await ParseFromModel(npc)));
        return Ok(new RetrieveNpcResponse { Npcs = npcs});
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
        var models = await Task.WhenAll(request.Npcs.Select(async npc =>
        {
            var model = await ParseBackToModel(npc, request.GameId, _npcService);
            model.NPCId = Guid.NewGuid();
            return model;
        }));
        foreach (var model in models)
        {
            await _npcService.PostNpc(model);
        }
        var npcs = await Task.WhenAll(models.Select(async model => await ParseFromModel(model)));
        return Ok(new CreateNpcResponse { Npcs = npcs });
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
        var models = await Task.WhenAll(request.Npcs.Select(async npc =>
        {
            var model = await ParseBackToModel(npc, request.GameId, _npcService);
            model.NPCId = Guid.NewGuid();
            return model;
        }));
        foreach (var model in models)
        {
            await _npcService.UpdateNpc(model);
        }
        var npcs = await Task.WhenAll(models.Select(async model => await ParseFromModel(model)));
        return Ok(new UpdateNpcResponse { Npcs = npcs });
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
        return Ok();
    }

    [HttpDelete("delete/all")]
    public async Task<ActionResult> DeleteNpcsInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteNpcRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        await _npcService.DeleteNpcByGameId(request.GameId);
        return Ok();
    }
}
