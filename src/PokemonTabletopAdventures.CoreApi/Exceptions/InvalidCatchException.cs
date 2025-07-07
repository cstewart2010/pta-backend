using PokemonTabletopAdventures.CoreApi.Constants;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class InvalidCatchException(string message, HttpStatusCode statusCode) : PtaException(message, PtaExceptionParts.InvalidCatchTitle, statusCode)
{
}
