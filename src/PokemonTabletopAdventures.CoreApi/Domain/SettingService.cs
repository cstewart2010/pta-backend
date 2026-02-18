using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class SettingService(
    IRepositoryService repositoryService,
    IShopService shopService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<SettingService> logger) : AbstractMongoService<SettingDto>(repositoryService, MongoCollection.Settings), ISettingService
{
    public async Task DeleteSetting(Guid id)
    {
        await ThrowIfNull(
            id,
            settingId => Collection.DeleteAsync(setting => setting.SettingId == settingId),
            PropertyNames.SettingId);
    }

    public async Task DeleteSettingsByGameId(Guid gameId)
    {
        await Collection.DeleteManyAsync(setting => setting.GameId == gameId);
    }

    public async Task<Setting?> GetActiveSetting(Guid gameId, bool isGM)
    {
        var dto = await Collection.GetOneAsync(setting => setting.GameId == gameId && setting.IsActive);
        if (dto == null)
        {
            return null;
        }

        return await dtoToModelMapper.ParseFromDto(dto, isGM, gameId, shopService);
    }

    public async Task<ICollection<Setting>> GetAllSettings(Guid gameId)
    {
        var dtos = await ThrowIfNullOrEmpty(
            gameId,
            id => Collection.GetManyAsync(setting => setting.GameId == gameId),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, true, gameId, shopService)));
    }

    public async Task<Setting> GetSetting(Guid settingId, bool isGM)
    {
        var dto = await ThrowIfNull(
            settingId,
            id => Collection.GetOneAsync(setting => setting.SettingId == settingId && (setting.IsActive || isGM)),
            PropertyNames.SettingId);

        return await dtoToModelMapper.ParseFromDto(dto, isGM, dto.GameId, shopService);
    }

    public async Task PostSetting(Setting setting)
    {
        var dto = await modelToDtoMapper.ParseFromModel(setting);
        await PostUniqueDocument(dto, x => x.SettingId == setting.SettingId);
    }

    public async Task<Setting> UpdateSetting(Setting updatedSetting, bool isGM)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedSetting);
        await UpsertDocument(
            setting => setting.SettingId,
            updatedSetting.SettingId,
            dto);

        return await GetSetting(dto.SettingId, isGM);
    }
}
