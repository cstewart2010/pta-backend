using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/movedex")]
public class MoveController(IDexService dexService, ILogger<MoveController> logger) : ControllerBase
{
    private const DexType Type = DexType.Moves;
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<MoveController> _logger = logger;

    [HttpGet(Name = nameof(GetMoves))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetMoves(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _dexService.GetStaticCollectionResponse<MoveModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetMove))]
    [ProducesResponseType(typeof(MoveModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMove(string name)
    {
        var document = await _dexService.GetDexEntry<MoveModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
