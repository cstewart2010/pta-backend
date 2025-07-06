using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Indicies;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.MovedexRoute)]
public class MoveController(IDexService dexService, ILogger<MoveController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Moves;
    private readonly ILogger<MoveController> _logger = logger;

    [HttpGet(Name = nameof(GetMoves))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetMoves(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<MoveModel>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetMove))]
    [ProducesResponseType(typeof(MoveModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMove(string name)
    {
        return await GetItem<MoveModel>(Type, name);
    }
}
