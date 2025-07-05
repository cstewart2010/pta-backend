using MongoDB.Driver;

namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class PtaMongoException(MongoWriteException exception) : PtaException(exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString(), "MongoDB Exception", System.Net.HttpStatusCode.BadRequest)
    {
    }
}
