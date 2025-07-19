using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;

namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class PtaMongoException(MongoWriteException exception) : PtaException(exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString(), PtaExceptionParts.MongoDbErrorTitle, System.Net.HttpStatusCode.BadRequest)
{
}
