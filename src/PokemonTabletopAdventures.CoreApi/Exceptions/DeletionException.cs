using PokemonTabletopAdventures.CoreApi.Constants;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class DeletionException(string message) : PtaException(message, PtaExceptionParts.DeletionErrorTitle, HttpStatusCode.BadRequest)
{
}
