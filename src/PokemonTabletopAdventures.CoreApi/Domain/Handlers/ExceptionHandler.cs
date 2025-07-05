using System;

namespace PokemonTabletopAdventures.CoreApi.Domain.Handlers;

internal static class ExceptionHandler
{
    public static ArgumentException IsNullOrEmpty(string argumentName)
    {
        return new ArgumentException("String value was null or empty", argumentName);
    }
}
