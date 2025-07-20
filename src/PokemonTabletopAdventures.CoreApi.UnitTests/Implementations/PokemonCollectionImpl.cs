using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class PokemonCollectionImpl : BaseCollectionImpl<PokemonDto>
{
    public override ICollection<PokemonDto> Collection { get; set; } = Shared.GameIds.Aggregate(new List<PokemonDto>(), (current, next) =>
    {
        var trainerItems = Shared.UserIds.Aggregate(new List<PokemonDto>(), (innerCurrent, innerNext) =>
        {
            int moveIndex = 0;
            var items = Shared.Pokemon.Select(x =>
            {
                var id = Guid.NewGuid();
                return new PokemonDto
                {
                    GameId = next,
                    TrainerId = innerNext,
                    PokemonId= id,
                    SpeciesName = x.Name,
                    EvolvedFrom = x.EvolvesFrom,
                    Moves = [Shared.PokemonMoves[moveIndex++]],
                    Form = x.Form,
                    DexNo = x.DexNo
                };
            });
            innerCurrent.AddRange(items);
            return innerCurrent;
        });
        current.AddRange(trainerItems);
        return current;
    });
}
