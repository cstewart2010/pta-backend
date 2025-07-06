using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class PtaUnauthorizedException(string message) : PtaException(message, PtaExceptionParts.AuthenticationErrorTitle, System.Net.HttpStatusCode.Unauthorized)
{
}
