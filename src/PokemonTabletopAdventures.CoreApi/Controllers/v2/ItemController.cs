using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.ItemdexRoute)]
public class ItemController(IDexService dexService, ILogger<ItemController> logger) : IndexControllerBase(dexService)
{
    [HttpGet("key", Name = nameof(GetKeyItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetKeyItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemDto, ItemController>(DexType.KeyItems, logger, offset, limit);
    }

    [HttpGet("key/{name}", Name = nameof(GetKeyItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetKeyItem(string name)
    {
        return await GetItem<BaseItemDto, ItemController>(DexType.KeyItems, logger, name);
    }

    [HttpGet("medical", Name = nameof(GetMedicalItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetMedicalItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemDto, ItemController>(DexType.MedicalItems, logger, offset, limit);
    }

    [HttpGet("medical/{name}", Name = nameof(GetMedicalItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetMedicalItem(string name)
    {
        return await GetItem<BaseItemDto, ItemController>(DexType.MedicalItems, logger, name);
    }

    [HttpGet("pokeball", Name = nameof(GetPokeballs))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetPokeballs(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemDto, ItemController>(DexType.Pokeballs, logger, offset, limit);
    }

    [HttpGet("pokeball/{name}", Name = nameof(GetPokeball))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokeball(string name)
    {
        return await GetItem<BaseItemDto, ItemController>(DexType.Pokeballs, logger, name);
    }

    [HttpGet("pokemon", Name = nameof(GetPokemonItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetPokemonItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemDto, ItemController>(DexType.PokemonItems, logger, offset, limit);
    }

    [HttpGet("pokemon/{name}", Name = nameof(GetPokemonItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetPokemonItem(string name)
    {
        return await GetItem<BaseItemDto, ItemController>(DexType.PokemonItems, logger, name);
    }

    [HttpGet("trainer", Name = nameof(GetTrainerItems))]
    [ProducesResponseType(typeof(IndexCollectionResponse), 200)]
    public async Task<IActionResult> GetTrainerItems(
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        return await GetItems<BaseItemDto, ItemController>(DexType.TrainerEquipment, logger, offset, limit);
    }

    [HttpGet("trainer/{name}", Name = nameof(GetTrainerItem))]
    [ProducesResponseType(typeof(IndexResponse<BaseItemDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerItem(string name)
    {
        return await GetItem<BaseItemDto, ItemController>(DexType.TrainerEquipment, logger, name);
    }
}
