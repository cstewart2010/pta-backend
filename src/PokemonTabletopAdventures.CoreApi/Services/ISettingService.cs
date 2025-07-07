using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ISettingService
{
    /// <summary>
    /// Returns an active encounter (if any) matching the game session id
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task<Setting?> GetActiveSetting(Guid gameId, bool isGM);

    /// <summary>
    /// Returns an setting matching the id
    /// </summary>
    /// <param name="encounterId">The setting id</param>
    public Task<Setting> GetSetting(Guid encounterId, bool isGM);

    /// <summary>
    /// Returns all settings associated with the game session
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task<IEnumerable<Setting>> GetAllSettings(Guid gameId);

    /// <summary>
    /// Attempts to add a encounter using the provided document
    /// </summary>
    /// <param name="encounter">The document to add</param>
    public Task PostSetting(Setting encounter);

    /// <summary>
    /// Attempts to replace the previous encounter with the new data
    /// </summary>
    /// <param name="updatedSetting">The updated encounter data</param>
    public Task<Setting> UpdateSetting(Setting updatedSetting, bool isGM);

    /// <summary>
    /// Searches for encounter using their game id, then deletes them
    /// </summary>
    /// <param name="gameId">The game session id</param>
    public Task DeleteSettingsByGameId(Guid gameId);

    /// <summary>
    /// Searches for a encounter using its id, then deletes it
    /// </summary>
    /// <param name="id">The encounter id</param>
    public Task DeleteSetting(Guid id);
}
