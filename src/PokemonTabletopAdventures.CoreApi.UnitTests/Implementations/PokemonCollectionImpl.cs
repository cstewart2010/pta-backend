using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class PokemonCollectionImpl : BaseCollectionImpl<PokemonDto>
{
    public override ICollection<PokemonDto> Collection { get; protected set; } = Shared.PokemonIds.Zip(Shared.GameIds).Aggregate(new List<PokemonDto>(), (current, next) =>
    {
        var trainerItems = Shared.UserIds.Aggregate(new List<PokemonDto>(), (innerCurrent, innerNext) =>
        {
            var items = Shared.Pokemon.Select((x, moveIndex) =>
            {
                var id = next.First;
                return new PokemonDto
                {
                    GameId = next.Second,
                    TrainerId = innerNext,
                    PokemonId= id,
                    SpeciesName = x.Name,
                    EvolvedFrom = x.EvolvesFrom,
                    Moves = [Shared.PokemonMoves[moveIndex]],
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
