using System;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class DuplicateEntryException(Type type) : PtaException($"Duplicate entry of type {type.Name}", "Duplicate Entry", HttpStatusCode.BadRequest)
    {
    }
}
