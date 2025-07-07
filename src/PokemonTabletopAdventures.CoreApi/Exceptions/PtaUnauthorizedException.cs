using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class PtaUnauthorizedException(string message) : PtaException(message, PtaExceptionParts.AuthenticationErrorTitle, System.Net.HttpStatusCode.Unauthorized)
{
}
