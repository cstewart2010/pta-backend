using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class DuplicateEntryException(Type type) : PtaException($"Duplicate entry of type {type.Name}", PtaExceptionParts.DuplicateEntryTitle, HttpStatusCode.BadRequest)
{
}
