using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Users;

public class User
{
    internal User(UserModel user)
    {
        UserId = user.UserId;
        Username = user.Username;
        DateCreated = user.DateCreated;
        Games = user.Games;
        Messages = user.Messages;
    }

    public Guid UserId { get; set; }

    public string Username { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public IEnumerable<Guid> Games { get; set; }

    public IEnumerable<Guid> Messages { get; set; }
}
