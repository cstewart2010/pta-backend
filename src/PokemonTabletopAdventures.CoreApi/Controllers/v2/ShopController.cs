using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Shops;

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

    [HttpPost("gm")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopGM(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        var shop = await _shopService.GetShopById(request.ShopId, request.GameId);
        return Ok(new RetrieveShopResponse { Shops = [shop] });
    }

    [HttpPost("trainer")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.UserId);
        var shop = await _shopService.GetShopById(request.ShopId, request.GameId);
        if (shop.IsActive != true)
        {
            throw new UnknownEntityException<Shop>(PropertyNames.ShopId, request.ShopId);
        }

        return Ok(new RetrieveShopResponse { Shops = [shop] });
    }

    [HttpPost("all")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShops(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        var shops = await _shopService.GetShopsByGameId(request.GameId);
        return Ok(new RetrieveShopResponse { Shops = [..shops]});
    }

    [HttpPost("setting/gm")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopsBySettingGM(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetSetting(request.SettingId, true);
        if (setting.GameId != request.GameId)
        {
            throw new InvalidSettingException($"The request setting {request.SettingId} is associated with game {request.GameId}");
        }

        var shops = await _shopService.GetShopsBySetting(setting);
        return Ok(new RetrieveShopResponse { Shops = [.. shops] });
    }

    [HttpPost("setting/trainer")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetShopsBySettingTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.UserId);
        var setting = await _settingService.GetSetting(request.SettingId, true);
        var shops = await _shopService.GetShopsBySetting(setting);
        return Ok(new RetrieveShopResponse { Shops = [.. shops.Where(shop => shop.IsActive)] });
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreateShopRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        foreach (var shop in request.Shops)
        {
            shop.GameId = request.GameId;
            shop.ShopId = Guid.NewGuid();
            await _shopService.PostShop(shop);
        }
        return Ok(new CreateShopResponse { Shops = request.Shops });
    }

    [HttpPut("update")]
    [ProducesResponseType(typeof(UpdateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        foreach (var requestShop in request.Shops)
        {
            var shop = await _shopService.GetShopById(requestShop.ShopId, request.GameId);
            shop.Name = requestShop.Name ?? shop.Name;
            shop.Inventory = requestShop.Inventory ?? shop.Inventory;
            shop.IsActive = requestShop.IsActive;
            await _shopService.UpdateShop(shop);
        }
        var shops = await _shopService.GetShopsByGameId(request.GameId);
        return Ok(new UpdateShopResponse { Shops = [..shops] });
    }

    [HttpPut("purchase")]
    [ProducesResponseType(typeof(UpdateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> PurchaseFromShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateShopRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.UserId);
        var requestShop = request.Shops.SingleOrDefault() ?? throw new InvalidSettingException(PtaExceptionParts.TooManyShopsMessage);
        var shop = await _shopService.GetShopById(requestShop.ShopId, request.GameId);
        if (shop.IsActive != true)
        {
            throw new InvalidShopException($"No active shop found with id: {requestShop.ShopId}");
        }
        var game = await GameService.GetGame(shop.GameId, false);
        var trainer = await TrainerService.GetTrainerById(request.UserId, shop.GameId);
        var (validWares, cost) = GetValidWares(shop, request.Items);

        if (cost > trainer.Money)
        {
            throw new InvalidShopException(PtaExceptionParts.BrokeNeighborMessage);
        }
        foreach (var ware in validWares.Where(ware => shop.Inventory[ware.Name].Quantity != -1))
        {
            shop.Inventory[ware.Name].Quantity -= ware.Amount;
        }
        await _shopService.UpdateShop(shop);

        trainer.Money -= cost;
        var logs = await AddItemsToTrainer(trainer, validWares);
        await GameService.UpdateGameLogs(game, false, [.. logs]);
        return Ok(new UpdateShopResponse
        {
            Trainer = trainer
        });
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteShop(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        await _shopService.DeleteShop(request.ShopId, request.GameId);
        return Ok();
    }

    [HttpDelete("delete/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> DeleteShopsByGameId(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, accessToken, sessionAuth);
        await _shopService.DeleteShopByGameId(request.GameId);
        return Ok();
    }

    private static (IEnumerable<Item> ValidWares, int Cost) GetValidWares(Shop shop, ICollection<Item> itemList)
    {
        var validWares = itemList.Where(item => CheckWare(item, shop));
        var cost = validWares.Aggregate(0, (currentCost, nextWare) =>
        {
            return currentCost + shop.Inventory[nextWare.Name].Cost * nextWare.Amount;
        });

        return (validWares, cost);
    }

    private static bool CheckWare(Item item, Shop shop)
    {
        var ware = shop.Inventory.FirstOrDefault(ware => item.Name == ware.Key && item.Type == ware.Value.Type);
        return !(ware.Key == null || item.Amount <= 0 || ware.Value.Quantity != -1 && item.Amount > ware.Value.Quantity);
    }
}
