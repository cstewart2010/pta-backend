using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class UnknownEntityException<T>(string entityName, object? entityValue) : PtaException($"Could not find a {typeof(T).Name} using {entityName}={entityValue}", "Unknown Entity", HttpStatusCode.NotFound)
    {
    }
}
