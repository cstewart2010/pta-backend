using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IShopService
{
    /// <summary>
    /// Returns a shop matching the id
    /// </summary>
    /// <param name="id">The shop id</param>
    /// <param name="gameId">The game id</param>
    public Task<ShopModel> GetShopById(Guid id, Guid gameId);

    /// <summary>
    /// Returns a shop contained in the game
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task<IEnumerable<ShopModel>> GetShopsByGameId(Guid gameId);

    /// <summary>
    /// Returns a shops matches contained in the setting
    /// </summary>
    /// <param name="setting">The setting in the game</param>
    public Task<IEnumerable<ShopModel>> GetShopsBySetting(SettingModel setting);

    /// <summary>
    /// Attempts to add an shop using the provided document
    /// </summary>
    /// <param name="shop">The document to add</param>
    public Task PostShop(ShopModel shop);

    /// <summary>
    /// Attempts to replace the previous shop with the new data
    /// </summary>
    /// <param name="updatedShop">the update shop data</param>
    public Task<ShopModel> UpdateShop(ShopModel updatedShop);

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
