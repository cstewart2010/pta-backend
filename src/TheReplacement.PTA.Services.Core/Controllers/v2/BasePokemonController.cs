using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/pokedex")]
public class BasePokemonController(IDexService basePokemonService, ILogger<BasePokemonController> logger) : ControllerBase
{
    private const DexType Type = DexType.BasePokemon;
    private readonly IDexService _basePokemonService = basePokemonService;
    private readonly ILogger<BasePokemonController> _logger = logger;

    [HttpGet(Name = nameof(GetPokemon))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetPokemon(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        var response = await _basePokemonService.GetStaticCollectionResponse<BasePokemonModel>(Type, offset, limit);
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetPokemonByName))]
    [ProducesResponseType(typeof(BasePokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByName(string name)
    {
        var document = await _basePokemonService.GetDexEntry<BasePokemonModel>(Type, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }

    [HttpGet("form/{form}", Name = nameof(GetPokemonByForm))]
    [ProducesResponseType(typeof(FormDataResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByForm(string form)
    {
        var entries = await _basePokemonService.GetDexEntries<BasePokemonModel>(Type);
        var formData = entries.Where(pokemon => pokemon.Form.Contains(form))
            .Select(pokemon => new FormData
            {
                Name = pokemon.Name,
                Form = pokemon.Form
            }).ToList();
        if (formData.Count != 0)
        {
            return Ok(new FormDataResponse { Data = formData });
        }

        throw new ItemNotFoundException(form);
    }
}
