using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

/// <summary>
/// Provides a collection of properties for MongoDB documents
/// </summary>
public interface IDocument
{
    /// <summary>
    /// MongoDB id
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }
}
