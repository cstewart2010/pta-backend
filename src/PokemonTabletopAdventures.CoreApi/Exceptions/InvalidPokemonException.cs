using PokemonTabletopAdventures.CoreApi.Constants;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class InvalidPokemonException(string message) : PtaException(message, PtaExceptionParts.InvalidPokemonTitle, HttpStatusCode.BadRequest)
{
}
