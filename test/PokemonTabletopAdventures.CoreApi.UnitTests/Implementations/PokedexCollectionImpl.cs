using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class PokedexCollectionImpl : BaseCollectionImpl<PokeDexItemDto>
{
    public override ICollection<PokeDexItemDto> Collection { get; protected set; } = Shared.GameIds.Aggregate(new List<PokeDexItemDto>(), (current, next) =>
    {
        var trainerItems = Shared.UserIds.Aggregate(new List<PokeDexItemDto>(), (innerCurrent, innerNext) =>
        {
            var items = Enumerable.Range(1, 3).Select(x =>
            {
                return new PokeDexItemDto
                {
                    GameId = next,
                    TrainerId = innerNext,
                    DexNo = x,
                    IsSeen = true,
                    IsCaught = Random.Shared.Next(2) == 0,
                };
            });
            innerCurrent.AddRange(items);
            return innerCurrent;
        });
        current.AddRange(trainerItems);
        return current;
    });
}
