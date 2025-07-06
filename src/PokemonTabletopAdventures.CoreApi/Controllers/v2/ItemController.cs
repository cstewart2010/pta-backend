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
[Route(Routes.ItemdexRoute)]
public class ItemController(IDexService dexService, ILogger<ItemController> logger) : IndexControllerBase(dexService)
{
    private readonly ILogger<ItemController> _logger = logger;

    [HttpGet("key", Name = nameof(GetKeyItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetKeyItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.KeyItems, offset, limit);
    }

    [HttpGet("key/{name}", Name = nameof(GetKeyItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetKeyItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.KeyItems, name);
    }

    [HttpGet("medical", Name = nameof(GetMedicalItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetMedicalItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.MedicalItems, offset, limit);
    }

    [HttpGet("medical/{name}", Name = nameof(GetMedicalItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMedicalItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.MedicalItems, name);
    }

    [HttpGet("pokeball", Name = nameof(GetPokeballs))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetPokeballs(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.MedicalItems, offset, limit);
    }

    [HttpGet("pokeball/{name}", Name = nameof(GetPokeball))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokeball(string name)
    {
        return await GetItem<BaseItemModel>(DexType.Pokeballs, name);
    }

    [HttpGet("pokemon", Name = nameof(GetPokemonItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetPokemonItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.PokemonItems, offset, limit);
    }

    [HttpGet("pokemon/{name}", Name = nameof(GetPokemonItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.PokemonItems, name);
    }

    [HttpGet("trainer", Name = nameof(GetTrainerItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetTrainerItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemModel>(DexType.TrainerEquipment, offset, limit);
    }

    [HttpGet("trainer/{name}", Name = nameof(GetTrainerItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerItem(string name)
    {
        return await GetItem<BaseItemModel>(DexType.TrainerEquipment, name);
    }
}
