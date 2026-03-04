using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class ShopCollectionImpl : BaseCollectionImpl<ShopDto>
{
    public override ICollection<ShopDto> Collection { get; protected set; } = Shared.GameIds.Select((x, gameIndex) => Shared.ShopIds.Select((
        y, shopIndex) =>
    {
        return new ShopDto
        {
            ShopId = y,
            GameId = x,
            IsActive = gameIndex == shopIndex,
            Name = y.ToString(),
            Inventory = Enumerable.Range(0, 10).ToDictionary(y => y.ToString(), y => new WareDto
            {
                Quantity = Random.Shared.Next(1, 100),
                Effects = Guid.NewGuid().ToString(),
                Type = (StartingEquipmentType)Random.Shared.Next(5),
                Cost = Random.Shared.Next(1, 100),
            })
        };
    })).SelectMany(x => x).ToList();
}