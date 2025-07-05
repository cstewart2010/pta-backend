using System.Net;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class ItemNotFoundException(string itemName) : PtaException($"Could not find {itemName}", "Item was not found", HttpStatusCode.NotFound)
{
}
