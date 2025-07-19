using PokemonTabletopAdventures.CoreApi.Constants;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class InvalidEvolutionException(string message) : PtaException(message, PtaExceptionParts.EvolutionErrorTitle, HttpStatusCode.BadRequest)
{
}
