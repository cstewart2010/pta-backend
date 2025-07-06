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
[Route(Routes.FeaturedexRoute)]
public class FeatureController(IDexService dexService, ILogger<FeatureController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Features;
    private readonly ILogger<FeatureController> _logger = logger;

    [HttpGet(Name = nameof(GetFeatures))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetFeatures(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<FeatureModel>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetFeature))]
    [ProducesResponseType(typeof(IndexResponse<FeatureModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetFeature(string name)
    {
        return await GetItem<FeatureModel>(Type, name);
    }
}
