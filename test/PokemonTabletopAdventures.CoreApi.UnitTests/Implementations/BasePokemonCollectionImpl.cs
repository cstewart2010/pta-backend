using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class BasePokemonCollectionImpl : BaseCollectionImpl<BasePokemonDto>
{
    public override ICollection<BasePokemonDto> Collection { get; protected set; } = [.. Enumerable.Range(0,5).Select(x => new BasePokemonDto
        {
            DexNo = Shared.Pokemon[x].DexNo,
            Form = Shared.Pokemon[x].Form,
            Name = Shared.Pokemon[x].Name,
            EvolvesFrom = Shared.Pokemon[x].EvolvesFrom,
            Moves = [Shared.PokemonMoves[x]],
            Rarity = Shared.Pokemon[x].Rarity
        })];
}
