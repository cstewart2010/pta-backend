using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Settings;

public class Setting
{
    public required Guid SettingId { get; set; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
    public required SettingType Type { get; set; }
    public required string[] Environment { get; set; }
    public required IEnumerable<Shop> Shops { get; set; }

    internal static async Task<Setting> ParseFromModel(SettingModel model, bool isGM, Guid gameId, IShopService shopService)
    {
        IEnumerable<ShopModel> shopModels = await Task.WhenAll(model.Shops.Select(async id => await shopService.GetShopById(id, gameId)));
        shopModels = shopModels.Where(shopModel => isGM || shopModel.IsActive);
        return new Setting
        {
            SettingId = model.SettingId,
            Name = model.Name,
            IsActive = model.IsActive,
            Type = model.Type,
            Environment = model.Environment,
            Shops = shopModels.Select(Shop.ParseFromModel),
        };
    }
}

public class Shop
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, WareModel> Inventory { get; set; } = [];

    internal static Shop ParseFromModel(ShopModel model)
    {
        return new Shop
        {
            ShopId = model.ShopId,
            Name = model.Name,
            Inventory = model.Inventory,
        };
    }
}
