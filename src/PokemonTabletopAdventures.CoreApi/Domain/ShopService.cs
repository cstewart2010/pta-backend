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
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;
    private readonly ILogger<ShopService> _logger = logger;

    public async Task DeleteShop(Guid id, Guid gameId)
    {
        await ThrowIfNull(
            id,
            shopId => Collection.DeleteAsync(shop => shop.ShopId == shopId && shop.GameId == gameId),
            PropertyNames.ShopId);
    }

    public async Task DeleteShopByGameId(Guid gameId)
    {
        await Collection.DeleteManyAsync(shop => shop.GameId == gameId);
    }

    public async Task<ICollection<Shop>> GetShopsByGameId(Guid gameId)
    {
        var dtos = await ThrowIfNullOrEmpty(
            gameId,
            id => Collection.GetManyAsync(shop => shop.GameId == id),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task<Shop> GetShopById(Guid id, Guid gameId)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(shop => shop.GameId == gameId && shop.ShopId == id),
            PropertyNames.ShopId);

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<ICollection<Shop>> GetShopsBySetting(Setting setting)
    {
        var shopIds = setting.Shops.Select(s => s.ShopId);
        var dtos = await Collection.GetManyAsync(shop => shopIds.Contains(shop.ShopId) && setting.GameId == shop.GameId);
        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task PostShop(Shop shop)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(shop);
        await PostUniqueDocument(dto, x => x.GameId == shop.GameId && x.ShopId == shop.ShopId);
    }

    public async Task<Shop> UpdateShop(Shop updatedShop)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(updatedShop);
        await UpsertDocument(
            shop => shop.ShopId,
            updatedShop.ShopId,
            dto);

        return await GetShopById(dto.ShopId, dto.GameId);
    }
}
