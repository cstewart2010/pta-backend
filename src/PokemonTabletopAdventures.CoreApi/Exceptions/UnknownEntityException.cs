using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class UnknownEntityException<T>(string entityName, object? entityValue) : PtaException($"Could not find a {typeof(T).Name} using {entityName}={entityValue}", PtaExceptionParts.UnknownEntityTitle, HttpStatusCode.NotFound)
{
}
