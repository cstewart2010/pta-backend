using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class InvalidSettingException(string message) : PtaException(message, PtaExceptionParts.InvalidSettingTitle, System.Net.HttpStatusCode.BadRequest)
{
}
