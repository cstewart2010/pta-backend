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
    public async Task DeleteSetting(Guid id, Guid gameId)
    {
        logger.LogInformation("Deleting setting {id}", id);
        await ThrowIfNull(
            id,
            settingId => Collection.DeleteAsync(setting => setting.SettingId == settingId && setting.GameId == gameId, logger),
            nameof(Setting.SettingId));
    }

    public async Task DeleteSettingsByGameId(Guid gameId)
    {
        await Collection.DeleteManyAsync(setting => setting.GameId == gameId, logger);
    }

    public async Task<Setting?> GetActiveSetting(Guid gameId, bool isGm)
    {
        var dto = await Collection.GetOneAsync(setting => setting.GameId == gameId && setting.IsActive, logger);
        if (dto == null)
        {
            return null;
        }

        return await dtoToModelMapper.ParseFromDto(dto, isGm, gameId, shopService);
    }

    public async Task<ICollection<Setting>> GetAllSettings(Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(setting => setting.GameId == gameId, logger);
        return await Task.WhenAll(dtos.Select(async dto => await dtoToModelMapper.ParseFromDto(dto, true, gameId, shopService)));
    }

    public async Task<Setting> GetSetting(Guid settingId, Guid gameId, bool isGm)
    {
        var dto = await ThrowIfNull(
            settingId,
            id => Collection.GetOneAsync(setting => setting.SettingId == id && setting.GameId == gameId && (setting.IsActive || isGm), logger),
            nameof(Setting.SettingId));

        return await dtoToModelMapper.ParseFromDto(dto, isGm, dto.GameId, shopService);
    }

    public async Task PostSetting(Setting setting)
    {
        var dto = await modelToDtoMapper.ParseFromModel(setting);
        await PostUniqueDocument(dto, x => x.SettingId == setting.SettingId, logger);
    }

    public async Task<Setting> UpdateSetting(Setting updatedSetting, bool isGm)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedSetting);
        await UpsertDocument(
            setting => setting.SettingId,
            updatedSetting.SettingId,
            dto,
            logger);

        return await GetSetting(dto.SettingId, dto.GameId, isGm);
    }
}
