using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class SettingService(IShopService shopService) : AbstractMongoService<SettingDto>(MongoCollection.Settings), ISettingService
{
    private readonly IShopService _shopService = shopService;

    public async Task DeleteSetting(Guid id)
    {
        await ThrowIfNull(
            id,
            settingId => Collection.FindOneAndDelete(setting => setting.SettingId == settingId),
            PropertyNames.SettingId);
    }

    public async Task DeleteSettingsByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            id => Collection.FindOneAndDelete(setting => setting.GameId == id),
            PropertyNames.GameId);
    }

    public async Task<Setting?> GetActiveSetting(Guid gameId, bool isGM)
    {
        var dto = await Task.FromResult(Collection.Find(setting => setting.GameId == gameId && setting.IsActive).SingleOrDefault());
        if (dto == null)
        {
            return null;
        }

        return await DtoHandler.ParseFromDto(dto, isGM, gameId, _shopService);
    }

    public async Task<IEnumerable<Setting>> GetAllSettings(Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.Find(setting => setting.GameId == gameId && setting.IsActive).ToEnumerable(),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(async dto => await DtoHandler.ParseFromDto(dto, true, gameId, _shopService)));
    }

    public async Task<Setting> GetSetting(Guid settingId, bool isGM)
    {
        var dto = await ThrowIfNull(
            settingId,
            id => Collection.Find(setting => setting.SettingId == settingId && setting.IsActive).SingleOrDefault(),
            PropertyNames.SettingId);

        return await DtoHandler.ParseFromDto(dto, isGM, dto.GameId, _shopService);
    }

    public async Task PostSetting(Setting setting)
    {
        var dto = DtoHandler.ParseFromModel(setting);
        await PostDocument(dto);
    }

    public async Task<Setting> UpdateSetting(Setting updatedSetting, bool isGM)
    {
        var dto = DtoHandler.ParseFromModel(updatedSetting);
        await UpsertDocument(
            Builders<SettingDto>.Filter.Eq(setting => setting.SettingId, updatedSetting.SettingId),
            dto);

        return await GetSetting(dto.SettingId, isGM);
    }
}
