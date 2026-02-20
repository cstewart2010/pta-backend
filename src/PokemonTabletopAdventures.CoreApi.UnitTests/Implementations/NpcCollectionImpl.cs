using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class NpcCollectionImpl : BaseCollectionImpl<NpcDto>
{
    public override ICollection<NpcDto> Collection { get; protected set; } = [..Shared.NpcIds.Zip(Shared.GameIds).Select(x =>
    {
        return new NpcDto
        {
            NPCId = x.First,
            GameId = x.Second,
            Age = Random.Shared.Next(10, 100),
            TrainerName = x.First.ToString()
        };
    })];
}
