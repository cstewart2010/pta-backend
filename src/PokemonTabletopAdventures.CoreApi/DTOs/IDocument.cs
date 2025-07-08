using MongoDB.Bson;
using Newtonsoft.Json;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Provides a collection of properties for MongoDB documents
/// </summary>
internal interface IDocument
{
    /// <summary>
    /// MongoDB id
    /// </summary>
    [JsonIgnore]
    public ObjectId _id { get; set; }
}
