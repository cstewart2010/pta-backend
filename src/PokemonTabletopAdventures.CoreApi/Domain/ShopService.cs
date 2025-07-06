using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class ShopService : AbstractService<ShopModel>, IShopService
{
    public ShopService() : base(MongoCollection.Shops) { }

    public async Task DeleteShop(Guid id, Guid gameId)
    {
        await ThrowIfNull(
            id,
            shopId => Collection.FindOneAndDelete(shop => shop.ShopId == shopId && shop.GameId == gameId),
            PropertyNames.ShopId);
    }

    public async Task DeleteShopByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            id => Collection.FindOneAndDelete(shop => shop.GameId == id),
            PropertyNames.GameId);
    }

    public async Task<IEnumerable<ShopModel>> GetShopsByGameId(Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(shop => shop.GameId == id).ToEnumerable(),
            PropertyNames.GameId);
    }

    public async Task<ShopModel> GetShopById(Guid id, Guid gameId)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(shop => shop.GameId == gameId && shop.ShopId == id).SingleOrDefault(),
            PropertyNames.ShopId);
    }

    public async Task<IEnumerable<ShopModel>> GetShopsBySetting(SettingModel setting)
    {
        return await ThrowIfNull(
            setting,
            id => Collection.Find(shop => setting.Shops.Contains(shop.ShopId) && setting.GameId == shop.GameId).ToEnumerable(),
            PropertyNames.SettingShops);
    }

    public async Task PostShop(ShopModel shop)
    {
        await PostDocument(shop);
    }

    public async Task<ShopModel> UpdateShop(ShopModel updatedShop)
    {
        await UpsertDocument(
            Builders<ShopModel>.Filter.Eq(shop => shop.ShopId, updatedShop.ShopId),
            updatedShop);

        return updatedShop;
    }
}
