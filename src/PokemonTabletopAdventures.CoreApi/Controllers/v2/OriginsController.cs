using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.OrigindexRoute)]
public class OriginsController(IDexService dexService, ILogger<OriginsController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Origins;

    [HttpGet(Name = nameof(GetOrigins))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetOrigins(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<OriginDto, OriginsController>(Type, logger, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetOrigin))]
    [ProducesResponseType(typeof(BerryDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetOrigin(string name)
    {
        return await GetItem<OriginDto, OriginsController>(Type, logger, name);
    }
}
