using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.BerrydexRoute)]
public class BerryController(IDexService dexService, ILogger<BerryController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.Berries;

    [HttpGet(Name = nameof(GetBerries))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetBerries(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BerryDto, BerryController>(Type, logger, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetBerry))]
    [ProducesResponseType(typeof(IndexResponse<BerryDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetBerry(string name)
    {
        return await GetItem<BerryDto, BerryController>(Type, logger, name);
    }
}
