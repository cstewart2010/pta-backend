using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class ShopCollectionImpl : BaseCollectionImpl<ShopDto>
{
    public override ICollection<ShopDto> Collection { get; set; } = [..Shared.ShopIds.Zip(Shared.GameIds).Select(x =>
    {
        return new ShopDto
        {
            ShopId = x.First,
            GameId = x.Second,
            IsActive = false,
            Name = x.First.ToString(),
            Inventory = Enumerable.Range(0, 10).ToDictionary(y => y.ToString(), y => new WareDto
            {
                Quantity = Random.Shared.Next(1, 100),
                Effects = Guid.NewGuid().ToString(),
                Type = (StartingEquipmentType)Random.Shared.Next(5),
                Cost = Random.Shared.Next(1, 100),
            })
        };
    })];
}