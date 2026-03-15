using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class InvalidShopException(string message) : PtaException(message, PtaExceptionParts.InvalidShopTitle, System.Net.HttpStatusCode.BadRequest)
{
}
