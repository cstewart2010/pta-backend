using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.PokedexRoute)]
public class BasePokemonController(IDexService basePokemonService, ILogger<BasePokemonController> logger) : IndexControllerBase(basePokemonService)
{
    private const DexType Type = DexType.BasePokemon;
    private readonly IDexService _basePokemonService = basePokemonService;
    private readonly ILogger<BasePokemonController> _logger = logger;

    [HttpGet(Name = nameof(GetPokemon))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetPokemon(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BasePokemonDto>(Type, offset, limit);
    }

    [HttpGet("{name}", Name = nameof(GetPokemonByName))]
    [ProducesResponseType(typeof(IndexResponse<BasePokemonDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByName(string name)
    {
        return await GetItem<BasePokemonDto>(Type, name);
    }

    [HttpGet("form/{form}", Name = nameof(GetPokemonByForm))]
    [ProducesResponseType(typeof(FormDataResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByForm(string form)
    {
        var entries = await _basePokemonService.GetDexEntries<BasePokemonDto>(Type);
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
