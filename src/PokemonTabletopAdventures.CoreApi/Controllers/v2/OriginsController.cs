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
[Route(Routes.OrigindexRoute)]
public class OriginsController(IDexService dexService, ILogger<OriginsController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Origins;
    private readonly ILogger<OriginsController> _logger = logger;

    [HttpGet(Name = nameof(GetOrigins))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetOrigins(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<OriginModel>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetOrigin))]
    [ProducesResponseType(typeof(BerryModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetOrigin(string name)
    {
        return await GetItem<OriginModel>(Type, name);
    }
}
