using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IShopService
{
    /// <summary>
    /// Returns a shop matching the id
    /// </summary>
    /// <param name="id">The shop id</param>
    /// <param name="gameId">The game id</param>
    public Task<Shop> GetShopById(Guid id, Guid gameId);

    /// <summary>
    /// Returns a shop contained in the game
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task<IEnumerable<Shop>> GetShopsByGameId(Guid gameId);

    /// <summary>
    /// Returns a shops matches contained in the setting
    /// </summary>
    /// <param name="setting">The setting in the game</param>
    public Task<IEnumerable<Shop>> GetShopsBySetting(Setting setting);

    /// <summary>
    /// Attempts to add an shop using the provided document
    /// </summary>
    /// <param name="shop">The document to add</param>
    public Task PostShop(Shop shop);

    /// <summary>
    /// Attempts to replace the previous shop with the new data
    /// </summary>
    /// <param name="updatedShop">the update shop data</param>
    public Task<Shop> UpdateShop(Shop updatedShop);

    /// <summary>
    /// Searches for a shop using their id, then deletes it
    /// </summary>
    /// <param name="id">The shop id</param>
    /// <param name="gameId">The game id</param>
    public Task DeleteShop(Guid id, Guid gameId);

    /// <summary>
    /// Searches for all shops using the game id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task DeleteShopByGameId(Guid gameId);
}
