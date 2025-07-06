using System.Collections;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class ImportFailedException(params object[] errors) : PtaException(string.Join("\n", errors), PtaExceptionParts.OneOrMoreTitle, System.Net.HttpStatusCode.BadRequest)
{
}
