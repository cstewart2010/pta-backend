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
[Route("api/v2/origindex")]
public class OriginsController(IDexService dexService, ILogger<OriginsController> logger) : ControllerBase
{
    private const DexType Type = DexType.Origins;
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<OriginsController> _logger = logger;

    [HttpGet(Name = nameof(GetOrigins))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetOrigins(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _dexService.GetStaticCollectionResponse<OriginModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetOrigin))]
    [ProducesResponseType(typeof(BerryModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetOrigin(string name)
    {
        var document = await _dexService.GetDexEntry<OriginModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
