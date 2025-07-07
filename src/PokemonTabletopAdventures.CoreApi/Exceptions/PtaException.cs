using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class PtaException(string message, string title, HttpStatusCode statusCode) : Exception(message)
{
    public string Title { get; } = title;

    public HttpStatusCode StatusCode { get; } = statusCode;
}
