using System;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class UserNotFoundException(Guid userId) : PtaException(userId.ToString(), "User was not found", HttpStatusCode.NotFound)
    {
    }
}
