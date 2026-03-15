using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class UserMessageThreadCollectionImpl : BaseCollectionImpl<UserMessageThreadDto>
{
    public override ICollection<UserMessageThreadDto> Collection { get; protected set; } = [];
}