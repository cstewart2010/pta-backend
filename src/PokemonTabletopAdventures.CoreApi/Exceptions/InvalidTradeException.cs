using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class InvalidTradeException() : PtaException(PtaExceptionParts.SelfTradeMessage, PtaExceptionParts.InvalidTradeTitle, HttpStatusCode.BadRequest)
{
}
