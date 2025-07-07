using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class InvalidTrainerException(string message) : PtaException(message, PtaExceptionParts.InvalidTrainerTitle, System.Net.HttpStatusCode.BadRequest)
{
}
