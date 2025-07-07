using MongoDB.Bson;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Provides a collection of properties for MongoDB documents
/// </summary>
internal interface IDocument
{
    /// <summary>
    /// MongoDB id
    /// </summary>
    public ObjectId _id { get; set; }
}
