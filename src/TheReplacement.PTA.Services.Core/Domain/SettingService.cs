using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class SettingService : AbstractService<SettingModel>, ISettingService
{
    public SettingService() : base(MongoCollection.Settings) { }

    public async Task DeleteSetting(Guid id)
    {
        await ThrowIfNull(
            id,
            settingId => Collection.FindOneAndDelete(setting => setting.SettingId == settingId),
            "SettingId");
    }

    public async Task DeleteSettingsByGameId(Guid gameId)
    {
        await ThrowIfNull(
            gameId,
            id => Collection.FindOneAndDelete(setting => setting.GameId == id),
            "GameId");
    }

    public async Task<SettingModel?> GetActiveSetting(Guid gameId)
    {
        return await Task.FromResult(Collection.Find(setting => setting.GameId == gameId && setting.IsActive).SingleOrDefault());
    }

    public async Task<IEnumerable<SettingModel>> GetAllSettings(Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(setting => setting.GameId == gameId && setting.IsActive).ToEnumerable(),
            "GameId");
    }

    public async Task<SettingModel> GetSetting(Guid settingId)
    {
        return await ThrowIfNull(
            settingId,
            id => Collection.Find(setting => setting.SettingId == settingId && setting.IsActive).SingleOrDefault(),
            "SettingId");
    }

    public async Task PostSetting(SettingModel setting)
    {
        await PostDocument(setting);
    }

    public async Task<SettingModel> UpdateSetting(SettingModel updatedSetting)
    {
        await UpsertDocument(
            Builders<SettingModel>.Filter.Eq(setting => setting.SettingId, updatedSetting.SettingId),
            updatedSetting);

        return updatedSetting;
    }
}
