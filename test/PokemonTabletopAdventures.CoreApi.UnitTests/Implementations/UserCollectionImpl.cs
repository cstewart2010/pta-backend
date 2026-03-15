using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class UserCollectionImpl : BaseCollectionImpl<UserDto>
{
    public override ICollection<UserDto> Collection { get; protected set; } =
    [
        ..Shared.UserIds.Select(x => new UserDto
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
        }),
        ..Shared.AdminIds.Select(x => new UserDto
        {
            UserId = x,
            IsOnline = true,
            SiteRole = Models.Enums.UserRoleOnSite.SiteAdmin,
            Messages = [],
            DateCreated = DateTime.Now,
            Username = x.ToString(),
            Games = [],
            ActivityToken = "",
            PasswordHash = ""
        })
    ];
}
