using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class GameCollectionImpl : BaseCollectionImpl<GameDto>
{
    public override ICollection<GameDto> Collection { get; set; } = [..Shared.GameIds.Select(x =>
    {
        return new GameDto
        {
            GameId = x,
            IsOnline = true,
            Logs = [],
            Nickname = x.ToString(),
            NPCs = [],
            PasswordHash = ""
        }; 
    })];
}
