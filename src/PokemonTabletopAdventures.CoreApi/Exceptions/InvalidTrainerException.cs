using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class InvalidTrainerException(string message) : PtaException(message, PtaExceptionParts.InvalidTrainerTitle, System.Net.HttpStatusCode.BadRequest)
{
}
