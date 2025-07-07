using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class ShopService : AbstractService<ShopDto>, IShopService
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

    public async Task<IEnumerable<Shop>> GetShopsByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.Find(shop => shop.GameId == id).ToEnumerable(),
            PropertyNames.GameId);

        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task<Shop> GetShopById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(shop => shop.GameId == gameId && shop.ShopId == id).SingleOrDefault(),
            PropertyNames.ShopId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<Shop>> GetShopsBySetting(Setting setting)
    {
        var shopIds = setting.Shops.Select(s => s.ShopId);
        var dtos = await ThrowIfNull(
            setting,
            id => Collection.Find(shop => shopIds.Contains(shop.ShopId) && setting.GameId == shop.GameId).ToEnumerable(),
            PropertyNames.SettingShops);

        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task PostShop(Shop shop)
    {
        var dto = DtoHandler.ParseFromModel(shop);
        await PostDocument(dto);
    }

    public async Task<Shop> UpdateShop(Shop updatedShop)
    {
        var dto = DtoHandler.ParseFromModel(updatedShop);
        await UpsertDocument(
            Builders<ShopDto>.Filter.Eq(shop => shop.ShopId, updatedShop.ShopId),
            dto);

        return await GetShopById(dto.ShopId, dto.GameId);
    }
}
