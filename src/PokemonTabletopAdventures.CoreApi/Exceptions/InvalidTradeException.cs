using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

internal class InvalidTradeException() : PtaException(PtaExceptionParts.SelfTradeMessage, PtaExceptionParts.InvalidTradeTitle, HttpStatusCode.BadRequest)
{
}
