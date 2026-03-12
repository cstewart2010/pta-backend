using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class ShopService(
    IRepositoryService repositoryService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<ShopService> logger) : AbstractMongoService<ShopDto>(repositoryService, MongoCollection.Shops), IShopService
{
    public async Task DeleteShop(Guid id, Guid gameId)
    {
        logger.LogInformation("Deleting shop {id} from {gameId}", id, gameId);
        await ThrowIfNull(
            id,
            shopId => Collection.DeleteAsync(shop => shop.ShopId == shopId && shop.GameId == gameId, logger),
            nameof(Shop.ShopId));
    }

    public async Task DeleteShopByGameId(Guid gameId)
    {
        await Collection.DeleteManyAsync(shop => shop.GameId == gameId, logger);
    }

    public async Task<ICollection<Shop>> GetShopsByGameId(Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(shop => shop.GameId == gameId, logger);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task<Shop> GetShopById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            id,
            x => Collection.GetOneAsync(shop => shop.GameId == gameId && shop.ShopId == x, logger),
            nameof(Shop.ShopId));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<ICollection<Shop>> GetShopsBySetting(Setting setting)
    {
        var shopIds = setting.Shops.Select(s => s.ShopId);
        var dtos = await Collection.GetManyAsync(shop => shopIds.Contains(shop.ShopId) && setting.GameId == shop.GameId, logger);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task PostShop(Shop shop)
    {
        var dto = await modelToDtoMapper.ParseFromModel(shop);
        await PostUniqueDocument(dto, x => x.GameId == shop.GameId && x.ShopId == shop.ShopId, logger);
    }

    public async Task<Shop> UpdateShop(Shop updatedShop)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedShop);
        await UpsertDocument(
            shop => shop.ShopId,
            updatedShop.ShopId,
            dto,
            logger);

        return await GetShopById(dto.ShopId, dto.GameId);
    }
}
