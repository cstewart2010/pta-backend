using System.Net;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class ItemNotFoundException(string itemName) : PtaException($"Could not find {itemName}", PtaExceptionParts.ItemNotFoundTitle, HttpStatusCode.NotFound)
{
}
