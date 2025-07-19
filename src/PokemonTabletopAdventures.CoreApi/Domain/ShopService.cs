using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class ShopService(IRepositoryService repositoryService) : AbstractMongoService<ShopDto>(repositoryService, MongoCollection.Shops), IShopService
{
    public async Task DeleteShop(Guid id, Guid gameId)
    {
        await ThrowIfNull(
            id,
            shopId => Collection.DeleteAsync(shop => shop.ShopId == shopId && shop.GameId == gameId),
            PropertyNames.ShopId);
    }

    public async Task DeleteShopByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            id => Collection.DeleteAsync(shop => shop.GameId == id),
            PropertyNames.GameId);
    }

    public async Task<IEnumerable<Shop>> GetShopsByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.GetManyAsync(shop => shop.GameId == id),
            PropertyNames.GameId);

        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task<Shop> GetShopById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(shop => shop.GameId == gameId && shop.ShopId == id),
            PropertyNames.ShopId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<Shop>> GetShopsBySetting(Setting setting)
    {
        var shopIds = setting.Shops.Select(s => s.ShopId);
        var dtos = await ThrowIfNull(
            setting,
            id => Collection.GetManyAsync(shop => shopIds.Contains(shop.ShopId) && setting.GameId == shop.GameId),
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
            shop => shop.ShopId,
            updatedShop.ShopId,
            dto);

        return await GetShopById(dto.ShopId, dto.GameId);
    }
}
