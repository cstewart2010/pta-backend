using PokemonTabletopAdventures.CoreApi.Domain.Mappers;

namespace PokemonTabletopAdventures.CoreApi.UnitTests;

internal static class Shared
{
    public static readonly ICollection<Guid> GameIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly ICollection<Guid> PokemonIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly ICollection<Guid> UserIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly ICollection<Guid> NpcIds = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    public static readonly List<(int DexNo, string Name, string Form, string EvolvesFrom, string Rarity)> Pokemon = [
        (1, "Bulbasaur", "Base", "", "Common"),
        (2, "Ivysaur", "Base", "Bulbasaur", "Uncommon"),
        (3, "Venusaur", "Base", "Ivysaur", "Rare"),
        (3, "Venusaur", "Mega", "Ivysaur", "Rare"),
        (3, "Venusaur", "Gigantamax", "Ivysaur", "Rare")
    ];
    public static readonly List<string> PokemonMoves = ["Move 1", "Move 2", "Move 3", "Move 3", "Move 3"];
    public static readonly DtoToModelMapper DtoToModelMapper = new DtoToModelMapper();
    public static readonly ModelToDtoMapper ModelToDtoMapper = new ModelToDtoMapper();
}
