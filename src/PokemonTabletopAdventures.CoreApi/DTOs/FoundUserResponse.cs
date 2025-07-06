using PokemonTabletopAdventures.CoreApi.DTOs.Users;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class FoundUserResponse : AbstractDto
{
    internal FoundUserResponse(UserModel user)
    {
        Message = "Trainer was found";
        User = new User(user);
        IsAdmin = user.SiteRole == UserRoleOnSite.SiteAdmin;
    }

    public User User { get; }
    public bool IsAdmin { get; }
}
