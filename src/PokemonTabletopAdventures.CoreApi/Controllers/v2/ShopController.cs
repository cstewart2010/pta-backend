using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.Shops;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.ShopRoute)]
public class ShopController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IShopService shopService,
    IGameService gameService,
    IDexService dexService,
    ISettingService settingService,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    ILogger<ShopController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexService, pokedexService, encryptionService)
{
	private readonly ILogger<ShopController> _logger = logger;
	private readonly IShopService _shopService = shopService;
    private readonly ISettingService _settingService = settingService;

    [HttpGet("{gameId}/{gameMasterId}/{shopId}/gm")]
    [ProducesResponseType(typeof(ShopModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopGM(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid shopId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var shop = await _shopService.GetShopById(shopId, gameId);
        return Ok(shop);
    }

    [HttpGet("{gameId}/{trainerId}/{shopId}/trainer")]
    [ProducesResponseType(typeof(ShopModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid shopId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var shop = await _shopService.GetShopById(shopId, gameId);
        if (shop?.IsActive != true)
        {
            return NotFound(shopId);
        }

        return Ok(shop);
    }

    [HttpGet("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(IEnumerable<ShopModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShops(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var shops = await _shopService.GetShopsByGameId(gameId);
        return Ok(shops);
    }

    [HttpGet("{gameId}/{gameMasterId}/{settingId}/setting/gm")]
    [ProducesResponseType(typeof(IEnumerable<ShopModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopsBySettingGM(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid settingId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var setting = await _settingService.GetSetting(settingId);
        if (setting?.GameId != gameId)
        {
            throw new InvalidSettingException($"The request setting {settingId} is associated with game {gameId}");
        }

        var shops = await _shopService.GetShopsBySetting(setting);
        return Ok(shops);
    }

    [HttpGet("{gameId}/{gameMasterId}/{settingId}/setting/trainer")]
    public async Task<IActionResult> GetShopsBySettingTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid settingId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var setting = await _settingService.GetSetting(settingId);
        var shops = await _shopService.GetShopsBySetting(setting);
        return Ok(shops.Where(shop => shop.IsActive));
    }

    [HttpPost("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(ShopModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] ShopModel request,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        request.GameId = gameId;
        request.ShopId = Guid.NewGuid();
        await _shopService.PostShop(request);
        return Ok(request);
    }

    [HttpPut("{gameId}/{gameMasterId}/{shopId}/update")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] ShopModel request,
        Guid gameId,
        Guid gameMasterId,
        Guid shopId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var shop = await _shopService.GetShopById(shopId, gameId);
        shop.Name = request.Name;
        shop.Inventory = request.Inventory;
        shop.IsActive = request.IsActive;
        await _shopService.UpdateShop(shop);

        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/{shopId}/purchase")]
    [ProducesResponseType(typeof(PutShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> PurchaseFromShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] ICollection<ItemModel> items,
        Guid gameId,
        Guid trainerId,
        Guid shopId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var shop = await _shopService.GetShopById(shopId, gameId);
        if (shop?.IsActive != true)
        {
            throw new InvalidShopException($"No active shop found with id: {shopId}");
        }
        var game = await GameService.GetGame(gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var (validWares, cost) = GetValidWares(shop, items);

        if (cost > trainer.Money)
        {
            throw new InvalidShopException("Not enough money to purchase all items on list");
        }
        foreach (var ware in validWares.Where(ware => shop.Inventory[ware.Name].Quantity != -1))
        {
            shop.Inventory[ware.Name].Quantity -= ware.Amount;
        }
        await _shopService.UpdateShop(shop);

        trainer.Money -= cost;
        var logs = await AddItemsToTrainer(trainer, validWares);
        await GameService.UpdateGameLogs(game, [.. logs]);
        return Ok(new PutShopResponse
        {
            Trainer = await ParseFromModel(trainer),
            Shop = shop
        });
    }

    [HttpDelete("{gameId}/{gameMasterId}/{shopId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid shopId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await _shopService.DeleteShop(shopId, gameId);
        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(PutShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteShopsByGameId(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await _shopService.DeleteShopByGameId(gameId);
        return Ok();
    }

    private static (IEnumerable<ItemModel> ValidWares, int Cost) GetValidWares(ShopModel shop, ICollection<ItemModel> itemList)
    {
        var validWares = itemList.Where(item => CheckWare(item, shop));
        var cost = validWares.Aggregate(0, (currentCost, nextWare) =>
        {
            return currentCost + shop.Inventory[nextWare.Name].Cost * nextWare.Amount;
        });

        return (validWares, cost);
    }

    private static bool CheckWare(ItemModel item, ShopModel shop)
    {
        var ware = shop.Inventory.FirstOrDefault(ware => item.Name == ware.Key && item.Type == ware.Value.Type);
        return !(ware.Key == null || item.Amount <= 0 || ware.Value.Quantity != -1 && item.Amount > ware.Value.Quantity);
    }
}
