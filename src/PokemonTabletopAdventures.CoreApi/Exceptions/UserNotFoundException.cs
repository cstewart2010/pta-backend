using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class UserNotFoundException(Guid userId) : PtaException(userId.ToString(), PtaExceptionParts.UserNotFoundTitle, HttpStatusCode.NotFound)
{
}
