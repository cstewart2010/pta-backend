using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class SpriteCollectionImpl : BaseCollectionImpl<SpriteDto>
{
    public override ICollection<SpriteDto> Collection { get; protected set; } = Enumerable.Range(0, 10).Select(i =>
        new SpriteDto
        {
            FriendlyText = Guid.NewGuid().ToString(),
            Value = Guid.NewGuid().ToString()
        }).ToList();
}