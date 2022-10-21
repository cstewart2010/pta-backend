using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/shop")]
    public class ShopController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        [HttpGet("{gameId}/{gameMasterId}/{shopId}/gm")]
        public ActionResult<ShopModel> GetShopGM(Guid gameId, Guid gameMasterId, Guid shopId)
        {
            if(!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var shop = DatabaseUtility.FindShopById(shopId, gameId);
            if (shop == null)
            {
                return NotFound(shopId);
            }

            return Ok(shop);
        }

        [HttpGet("{gameId}/{trainerId}/{shopId}/trainer")]
        public ActionResult<ShopModel> GetShopTrainer(Guid gameId, Guid trainerId, Guid shopId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var shop = DatabaseUtility.FindShopById(shopId, gameId);
            if (shop?.IsActive != true)
            {
                return NotFound(shopId);
            }

            return Ok(shop);
        }

        [HttpGet("{gameId}/{gameMasterId}")]
        public ActionResult<IEnumerable<ShopModel>> GetShops(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var shops = DatabaseUtility.FindShopsByGameId(gameId);
            return Ok(shops);
        }

        [HttpGet("{gameId}/{gameMasterId}/{settingId}/setting/gm")]
        public ActionResult<IEnumerable<ShopModel>> GetShopsBySettingGM(Guid gameId, Guid gameMasterId, Guid settingId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var setting = DatabaseUtility.FindSetting(settingId);
            if (setting?.GameId != gameId)
            {
                return NotFound();
            }

            var shops = DatabaseUtility.FindShopsBySetting(setting);
            return Ok(shops);
        }

        [HttpGet("{gameId}/{gameMasterId}/{settingId}/setting/trainer")]
        public ActionResult<IEnumerable<ShopModel>> GetShopsBySettingTrainer(Guid gameId, Guid gameMasterId, Guid settingId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var setting = DatabaseUtility.FindSetting(settingId);
            if (setting?.GameId != gameId)
            {
                return NotFound();
            }

            var shops = DatabaseUtility
                .FindShopsBySetting(setting)
                .Where(shop => shop.IsActive);
            return Ok(shops);
        }

        [HttpPost("{gameId}/{gameMasterId}")]
        public async Task<ActionResult<ShopModel>> CreateShop(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var shop = await Request.GetRequestBody<ShopModel>();
            shop.GameId = gameId;
            shop.ShopId = Guid.NewGuid();
            if (!DatabaseUtility.TryAddShop(shop, out var error))
            {
                return BadRequest(error);
            }

            return Ok(shop);
        }

        [HttpPut("{gameId}/{gameMasterId}/{shopId}/update")]
        public async Task<ActionResult<ShopModel>> UpdateShop(Guid gameId, Guid gameMasterId, Guid shopId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var shop = DatabaseUtility.FindShopById(shopId, gameId);
            var data = await Request.GetRequestBody<ShopModel>();
            shop.Name = data.Name;
            shop.Inventory = data.Inventory;
            shop.IsActive = data.IsActive;
            if (!DatabaseUtility.UpdateShop(shop))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/{shopId}/purchase")]
        public async Task<ActionResult> PurchaseFromShop(Guid gameId, Guid trainerId, Guid shopId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var shop = DatabaseUtility.FindShopById(shopId, gameId);
            if (shop?.IsActive != true)
            {
                return NotFound(shopId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            var (validWares, cost) = await GetValidWares(shop);

            if (cost > trainer.Money)
            {
                return BadRequest("not enough money");
            }
            foreach (var ware in validWares.Where(ware => shop.Inventory[ware.Name].Quantity != -1))
            {
                shop.Inventory[ware.Name].Quantity -= ware.Amount;
            }

            if (!DatabaseUtility.UpdateShop(shop))
            {
                return BadRequest();
            }

            trainer.Money -= cost;
            var logs = AddItemsToTrainer(trainer, validWares);
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), logs.ToArray());
            return Ok(new
            {
                trainer = new PublicTrainer(trainer),
                shop
            });
        }

        [HttpDelete("{gameId}/{gameMasterId}/{shopId}")]
        public ActionResult DeleteShop(Guid gameId, Guid gameMasterId, Guid shopId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteShop(shopId, gameId))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}")]
        public ActionResult DeleteShopsByGameId(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteShopByGameId(gameId))
            {
                return BadRequest();
            }

            return Ok();
        }

        private async Task<(IEnumerable<ItemModel> ValidWares, int Cost)> GetValidWares(ShopModel shop)
        {
            var groceries = await Request.GetRequestBody<IEnumerable<ItemModel>>();
            var validWares = groceries.Where(item => CheckWare(item, shop));
            var cost = validWares.Aggregate(0, (currentCost, nextWare) =>
            {
                return currentCost + shop.Inventory[nextWare.Name].Cost * nextWare.Amount;
            });

            return (validWares, cost);
        }

        private static bool CheckWare(ItemModel item, ShopModel shop)
        {
            var ware = shop.Inventory.FirstOrDefault(ware => item.Name == ware.Key && item.Type == ware.Value.Type);
            return !(ware.Key == null || item.Amount <= 0 || (ware.Value.Quantity != -1 && item.Amount > ware.Value.Quantity));
        }
    }
}
