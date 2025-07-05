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
[Route("api/v2/classdex")]
public class TrainerClassController(IDexService dexService, ILogger<TrainerClassController> logger) : ControllerBase
{
    private const DexType Type = DexType.TrainerClasses;
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<TrainerClassController> _logger = logger;

    [HttpGet(Name = nameof(GetClasses))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _dexService.GetStaticCollectionResponse<TrainerClassModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetClass))]
    [ProducesResponseType(typeof(TrainerClassModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetClass(string name)
    {
        var document = await _dexService.GetDexEntry<TrainerClassModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
