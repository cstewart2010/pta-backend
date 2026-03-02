using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class PokemonCollectionImpl : BaseCollectionImpl<PokemonDto>
{
    public override ICollection<PokemonDto> Collection { get; protected set; } = Shared.GameIds.Aggregate(new List<PokemonDto>(), (current, next) =>
    {
        var trainerItems = Shared.UserIds.Aggregate(new List<PokemonDto>(), (innerCurrent, innerNext) =>
        {
            var items = Shared.Pokemon.Select((x, moveIndex) =>
            {
                var id = Guid.NewGuid();
                return new PokemonDto
                {
                    AlternateForms = x.AlternateForms,
                    GameId = next,
                    TrainerId = innerNext,
                    PokemonId= id,
                    SpeciesName = x.Name,
                    EvolvedFrom = x.EvolvesFrom,
                    Moves = [Shared.PokemonMoves[moveIndex]],
                    Form = x.Form,
                    DexNo = x.DexNo,
                    IsOnActiveTeam = x.DexNo % 2 == 0,
                    PokemonStats = new Stats
                    {
                        HP = 50,
                        Speed = 5,
                    },
                    Nature = Nature.Adamant
                };
            });
            innerCurrent.AddRange(items);
            return innerCurrent;
        });
        current.AddRange(trainerItems);
        return current;
    });
}
