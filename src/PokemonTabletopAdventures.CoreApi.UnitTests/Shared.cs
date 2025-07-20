using PokemonTabletopAdventures.CoreApi.Domain.Mappers;

namespace PokemonTabletopAdventures.CoreApi.UnitTests;

internal static class Shared
{
    public static readonly ICollection<Guid> GameIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly ICollection<Guid> UserIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly DtoToModelMapper DtoToModelMapper = new DtoToModelMapper();
    public static readonly ModelToDtoMapper ModelToDtoMapper = new ModelToDtoMapper();
}
