using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class ConflictInDataException(string message) : PtaException(message, PtaExceptionParts.ConflictInDataTitle, HttpStatusCode.Conflict)
{
}
