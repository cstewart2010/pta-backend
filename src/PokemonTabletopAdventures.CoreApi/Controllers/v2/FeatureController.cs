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
[Route("api/v2/featuredex")]
public class FeatureController(IDexService dexService, ILogger<FeatureController> logger) : ControllerBase
{
    private const DexType Type = DexType.Features;
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<FeatureController> _logger = logger;

    [HttpGet(Name = nameof(GetFeatures))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetFeatures(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _dexService.GetStaticCollectionResponse<FeatureModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetFeature))]
    [ProducesResponseType(typeof(FeatureModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetFeature(string name)
    {
        var document = await _dexService.GetDexEntry<FeatureModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
