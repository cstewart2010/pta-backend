using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.TrainerClassRoute)]
public class TrainerClassController(IDexService dexService, ILogger<TrainerClassController> logger) : IndexControllerBase(dexService)
{
    private const DexType Type = DexType.TrainerClasses;
    private readonly ILogger<TrainerClassController> _logger = logger;

    [HttpGet(Name = nameof(GetClasses))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<TrainerClassDto>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetClass))]
    [ProducesResponseType(typeof(TrainerClassDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetClass(string name)
    {
        return await GetItem<TrainerClassDto>(Type, name);
    }
}
