using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class UserCollectionImpl : BaseCollectionImpl<UserDto>
{
    public override ICollection<UserDto> Collection { get; set; } = [..Shared.UserIds.Select(x =>
    {
        return new UserDto
        {
            UserId = x,
            IsOnline = true,
            SiteRole = Models.Enums.UserRoleOnSite.Active,
            Messages = [],
            DateCreated = DateTime.Now,
            Username = x.ToString(),
            Games = [],
            ActivityToken = "",
            PasswordHash = ""
        };
    })];
}
