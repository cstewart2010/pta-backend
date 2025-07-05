using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Interfaces;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/itemdex")]
public class ItemController(IDexService dexService, ILogger<ItemController> logger) : ControllerBase
{
    private readonly IDexService _dexService = dexService;
    private readonly ILogger<ItemController> _logger = logger;

    [HttpGet("key", Name = nameof(GetKeyItems))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetKeyItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.KeyItems, offset, limit);
    }

    [HttpGet("key/{name}", Name = nameof(GetKeyItem))]
    [ProducesResponseType(typeof(BaseItemModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetKeyItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.KeyItems, name);
    }

    [HttpGet("medical", Name = nameof(GetMedicalItems))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetMedicalItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.MedicalItems, offset, limit);
    }

    [HttpGet("medical/{name}", Name = nameof(GetMedicalItem))]
    [ProducesResponseType(typeof(BaseItemModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMedicalItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.MedicalItems, name);
    }

    [HttpGet("pokeball", Name = nameof(GetPokeballs))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetPokeballs(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.MedicalItems, offset, limit);
    }

    [HttpGet("pokeball/{name}", Name = nameof(GetPokeball))]
    [ProducesResponseType(typeof(BaseItemModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokeball(string name)
    {
        return await GetItem<BaseItemModel>(DexType.Pokeballs, name);
    }

    [HttpGet("pokemon", Name = nameof(GetPokemonItems))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetPokemonItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.PokemonItems, offset, limit);
    }

    [HttpGet("pokemon/{name}", Name = nameof(GetPokemonItem))]
    [ProducesResponseType(typeof(BaseItemModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.PokemonItems, name);
    }

    [HttpGet("trainer", Name = nameof(GetTrainerItems))]
    [ProducesResponseType(typeof(StaticCollectionResponse<string>), 200)]
    public async Task<IActionResult> GetTrainerItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.TrainerEquipment, offset, limit);
    }

    [HttpGet("trainer/{name}", Name = nameof(GetTrainerItem))]
    [ProducesResponseType(typeof(BaseItemModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.TrainerEquipment, name);
    }

    private async Task<OkObjectResult> GetItems<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDexDocument
    {
        var response = await _dexService.GetStaticCollectionResponse<TDocument>(documentType, offset, limit);
        return Ok(response);
    }

    private async Task<OkObjectResult> GetItem<TDocument>(
        DexType documentType,
        string name) where TDocument : IDexDocument
    {
        var document = await _dexService.GetDexEntry<TDocument>(documentType, name);
        if (document != null)
        {
            return Ok(document);
        }

        throw new ItemNotFoundException(name);
    }
}
