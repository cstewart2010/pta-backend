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
[Route(Routes.BerrydexRoute)]
public class BerryController(IDexService dexService, ILogger<BerryController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Berries;
    private readonly ILogger<BerryController> _logger = logger;

    [HttpGet(Name = nameof(GetBerries))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetBerries(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BerryModel>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetBerry))]
    [ProducesResponseType(typeof(IndexResponse<BerryModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBerry(string name)
    {
        return await GetItem<BerryModel>(Type, name);
    }
}
