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
[Route("api/v2/berrydex")]
public class BerryController(IDexService dexService, ILogger<BerryController> logger) : ControllerBase
{
    private const DexType Type = DexType.Berries;
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<BerryController> _logger = logger;

    [HttpGet(Name = nameof(GetBerries))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetBerries(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _dexService.GetStaticCollectionResponse<BerryModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetBerry))]
    [ProducesResponseType(typeof(BerryModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBerry(string name)
    {
        var document = await _dexService.GetDexEntry<BerryModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
