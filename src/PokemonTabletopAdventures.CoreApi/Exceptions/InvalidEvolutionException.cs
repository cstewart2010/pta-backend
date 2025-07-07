using PokemonTabletopAdventures.CoreApi.Constants;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class InvalidEvolutionException(string message) : PtaException(message, PtaExceptionParts.EvolutionErrorTitle, HttpStatusCode.BadRequest)
{
}
