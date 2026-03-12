using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.MovedexRoute)]
public class MoveController(IDexService dexService, ILogger<MoveController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Moves;

    [HttpGet(Name = nameof(GetMoves))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetMoves(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<MoveDto, MoveController>(Type, logger, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetMove))]
    [ProducesResponseType(typeof(MoveDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMove(string name)
    {
        return await GetItem<MoveDto, MoveController>(Type, logger, name);
    }
}
