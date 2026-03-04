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

    [HttpGet(Name = nameof(GetAllPokemon))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetAllPokemon()
    {
        logger.LogInformation("Retrieving all pokemon");
        var response = await DexService.GetOrderedIndexCollectionResponse();
        return Ok(response);
    }

    [HttpGet("{name}", Name = nameof(GetPokemonByName))]
    [ProducesResponseType(typeof(PokemonAndForms), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByName(string name)
    {
        var entries = await _basePokemonService.GetPokedexEntry(name, "Base");
        return Ok(entries);
    }

    [HttpGet("form/{form}", Name = nameof(GetPokemonByForm))]
    [ProducesResponseType(typeof(FormDataResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByForm(string form)
    {
        var entries = await _basePokemonService.GetDexEntries<BasePokemonDto>(Type);
        var formData = entries.Where(pokemon => pokemon.Form.Contains(form, StringComparison.InvariantCultureIgnoreCase))
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

    [HttpGet("{name}form/{form}", Name = nameof(GetPokemonByNameAndForm))]
    [ProducesResponseType(typeof(PokemonAndForms), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonByNameAndForm(string name, string form)
    {
        var entries = await _basePokemonService.GetPokedexEntry(name, form);
        return Ok(entries);
    }
}
