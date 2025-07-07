using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class UserNotFoundException(Guid userId) : PtaException(userId.ToString(), PtaExceptionParts.UserNotFoundTitle, HttpStatusCode.NotFound)
{
}
