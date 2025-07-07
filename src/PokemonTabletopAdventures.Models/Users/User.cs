using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models.Users;

public class User
{
    public required Guid UserId { get; set; }

    public required string Username { get; set; }

    public required DateTimeOffset DateCreated { get; set; }

    public required ICollection<Guid> Games { get; set; }

    public required ICollection<Guid> Messages { get; set; }

    public required UserRoleOnSite SiteRole { get; set; }
}
