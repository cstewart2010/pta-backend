using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class UpdateException(string message) : PtaException(message, PtaExceptionParts.UpdateErrorTitle, System.Net.HttpStatusCode.BadRequest)
{
}
