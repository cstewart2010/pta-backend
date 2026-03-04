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
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<ShopController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexService, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    [HttpPost("gm")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetShopGM(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        var shop = await shopService.GetShopById(request.ShopId, request.GameId);
        return Ok(new RetrieveShopResponse { Shops = [shop] });
    }

    [HttpPost("trainer")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetShopTrainer(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.UserId);
        var shop = await shopService.GetShopById(request.ShopId, request.GameId);
        CheckShopIsActive(shop);
        return Ok(new RetrieveShopResponse { Shops = [shop] });
    }

    [HttpPost("all")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetShops(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        var shops = await shopService.GetShopsByGameId(request.GameId);
        return Ok(new RetrieveShopResponse { Shops = [..shops]});
    }

    [HttpPost("setting/gm")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> GetShopsBySettingGM(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        var setting = await settingService.GetSetting(request.SettingId, request.GameId, true);
        var shops = await shopService.GetShopsBySetting(setting);
        return Ok(new RetrieveShopResponse { Shops = [.. shops] });
    }

    [HttpPost("setting/trainer")]
    [ProducesResponseType(typeof(RetrieveShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> GetShopsBySettingTrainer(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveShopRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.UserId);
        var setting = await settingService.GetSetting(request.SettingId, request.GameId, false);
        var shops = await shopService.GetShopsBySetting(setting);
        return Ok(new RetrieveShopResponse { Shops = [.. shops.Where(shop => shop.IsActive)] });
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> CreateShop(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreateShopRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, logger, sessionAuth);
        foreach (var shop in request.Shops)
        {
            shop.GameId = request.GameId;
            shop.ShopId = Guid.NewGuid();
            await shopService.PostShop(shop);
        }
        return Ok(new CreateShopResponse { Shops = request.Shops });
    }

    [HttpPut("update")]
    [ProducesResponseType(typeof(UpdateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> UpdateShop(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        foreach (var requestShop in request.Shops)
        {
            var shop = await shopService.GetShopById(requestShop.ShopId, request.GameId);
            shop.Name = requestShop.Name;
            shop.Inventory = requestShop.Inventory;
            shop.IsActive = requestShop.IsActive;
            await shopService.UpdateShop(shop);
        }
        var shops = await shopService.GetShopsByGameId(request.GameId);
        return Ok(new UpdateShopResponse { Shops = [..shops] });
    }

    [HttpPut("purchase")]
    [ProducesResponseType(typeof(UpdateShopResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> PurchaseFromShop(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateShopRequest request)
    {
        await VerifyIdentity(sessionAuth, logger, request.UserId);
        if (request.Shops.Count != 1)
        {
            throw new InvalidShopException(PtaExceptionParts.TooManyShopsMessage);
        }
        var requestShop = request.Shops.First();
        var shop = await shopService.GetShopById(requestShop.ShopId, request.GameId);
        CheckShopIsActive(shop);
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
        await shopService.UpdateShop(shop);

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
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult> DeleteShop(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        await shopService.DeleteShop(request.ShopId, request.GameId);
        return Ok();
    }

    [HttpDelete("delete/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult> DeleteShopsByGameId(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteShopRequest request)
    {
        await IsUserGM(request.UserId, request.GameId, logger, sessionAuth);
        await shopService.DeleteShopByGameId(request.GameId);
        return Ok();
    }

    private static (ICollection<Item> ValidWares, int Cost) GetValidWares(Shop shop, ICollection<Item> itemList)
    {
        var validWares = itemList.Where(item => CheckWare(item, shop)).ToList();
        var cost = validWares.Aggregate(0, (currentCost, nextWare) => currentCost + shop.Inventory[nextWare.Name].Cost * nextWare.Amount);
        return (validWares, cost);
    }

    private static bool CheckWare(Item item, Shop shop)
    {
        var ware = shop.Inventory.FirstOrDefault(ware => item.Name == ware.Key && item.Type == ware.Value.Type);
        return !(ware.Key == null || item.Amount <= 0 || ware.Value.Quantity != -1 && item.Amount > ware.Value.Quantity);
    }

    private static void CheckShopIsActive(Shop shop)
    {
        if (!shop.IsActive)
        {
            throw new UnknownEntityException<Shop>(PropertyNames.ShopId, shop.ShopId);
        }
    }
}
