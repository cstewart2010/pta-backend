using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class UpdateException(string message) : PtaException(message, PtaExceptionParts.UpdateErrorTitle, System.Net.HttpStatusCode.BadRequest)
{
}
