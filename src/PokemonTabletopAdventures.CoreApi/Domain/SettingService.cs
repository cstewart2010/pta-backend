using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class SettingService(
    IRepositoryService repositoryService,
    IShopService shopService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<SettingDto>(repositoryService, MongoCollection.Settings), ISettingService
{
    private readonly IShopService _shopService = shopService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

    public async Task DeleteSetting(Guid id)
    {
        await ThrowIfNull(
            id,
            settingId => Collection.DeleteAsync(setting => setting.SettingId == settingId),
            PropertyNames.SettingId);
    }

    public async Task DeleteSettingsByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            id => Collection.DeleteAsync(setting => setting.GameId == id),
            PropertyNames.GameId);
    }

    public async Task<Setting?> GetActiveSetting(Guid gameId, bool isGM)
    {
        var dto = await Collection.GetOneAsync(setting => setting.GameId == gameId && setting.IsActive);
        if (dto == null)
        {
            return null;
        }

        return await _dtoToModelMapper.ParseFromDto(dto, isGM, gameId, _shopService);
    }

    public async Task<ICollection<Setting>> GetAllSettings(Guid gameId)
    {
        var dtos = await ThrowIfNullOrEmpty(
            gameId,
            id => Collection.GetManyAsync(setting => setting.GameId == gameId && setting.IsActive),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await _dtoToModelMapper.ParseFromDto(dto, true, gameId, _shopService)));
    }

    public async Task<Setting> GetSetting(Guid settingId, bool isGM)
    {
        var dto = await ThrowIfNull(
            settingId,
            id => Collection.GetOneAsync(setting => setting.SettingId == settingId && (setting.IsActive || isGM)),
            PropertyNames.SettingId);

        return await _dtoToModelMapper.ParseFromDto(dto, isGM, dto.GameId, _shopService);
    }

    public async Task PostSetting(Setting setting)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(setting);
        await PostUniqueDocument(dto, x => x.SettingId == setting.SettingId);
    }

    public async Task<Setting> UpdateSetting(Setting updatedSetting, bool isGM)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(updatedSetting);
        await UpsertDocument(
            setting => setting.SettingId,
            updatedSetting.SettingId,
            dto);

        return await GetSetting(dto.SettingId, isGM);
    }
}
